using ARP.Entity.Cadastros;
using ARP.Infra;
using Microsoft.EntityFrameworkCore;

namespace ARP.Modules.Auth
{
    public sealed class PermissionAuthorizationService
    {
        public static IQueryable<long> AuthorizedCompanyIds(
            IQueryable<ARP.Entity.Cadastros.Empresa> companies,
            IQueryable<UsuarioEmpresa> memberships,
            IQueryable<UsuarioPersona> assignments,
            ActiveTenantContext activeContext,
            string permissionCode)
        {
            var allowedPersonas = AllowedPersonas(permissionCode);
            return companies.Where(company => company.Ativo
                && company.TenantId == activeContext.TenantId
                && memberships.Any(membership => membership.UsuarioId == activeContext.UserId
                    && membership.EmpresaId == company.Id && membership.Ativo)
                && assignments.Any(assignment => assignment.UsuarioId == activeContext.UserId
                    && assignment.TenantId == company.TenantId && assignment.Ativo
                    && (assignment.EmpresaId == null || assignment.EmpresaId == company.Id)
                    && assignment.Persona.Ativo && allowedPersonas.Contains(assignment.Persona.Codigo)
                    && assignment.Persona.Permissoes.Any(mapping => mapping.Permissao.Ativo
                        && mapping.Permissao.Codigo == permissionCode)))
                .Select(company => company.Id);
        }

        public async Task RequireCompaniesAsync(
            Context context, ActiveTenantContext activeContext, IEnumerable<long> companyIds,
            string permissionCode, CancellationToken cancellationToken)
        {
            if (activeContext.IsAdmin) return;
            var requestedIds = companyIds.Distinct().ToArray();
            var authorizedIds = await AuthorizedCompanyIds(context.Empresas, context.UsuariosEmpresas,
                context.UsuariosPersonas, activeContext, permissionCode)
                .Where(id => requestedIds.Contains(id)).ToArrayAsync(cancellationToken);
            if (requestedIds.Except(authorizedIds).Any())
                throw new GraphQLException(ErrorBuilder.New()
                    .SetMessage("Você não possui permissão para esta operação em todas as empresas da pesquisa.")
                    .SetCode("AUTH_NOT_AUTHORIZED").Build());
        }

        public async Task<bool> CanAsync(
            Context context,
            ActiveTenantContext activeContext,
            string permissionCode,
            CancellationToken cancellationToken)
        {
            if (activeContext.IsAdmin)
                return true;

            if (activeContext.TenantId is not long tenantId)
                return false;

            var assignments = context.UsuariosPersonas.Where(assignment =>
                assignment.UsuarioId == activeContext.UserId
                && assignment.TenantId == tenantId
                && assignment.Ativo
                && assignment.Persona.Ativo
                && (assignment.EmpresaId == null || assignment.EmpresaId == activeContext.CompanyId));

            var allowedPersonas = AllowedPersonas(permissionCode);
            if (allowedPersonas.Length == 0)
                return false;

            assignments = assignments.Where(assignment => allowedPersonas.Contains(assignment.Persona.Codigo));

            return await assignments.AnyAsync(assignment =>
                assignment.Persona.Permissoes.Any(mapping =>
                    mapping.Permissao.Ativo && mapping.Permissao.Codigo == permissionCode),
                cancellationToken);
        }

        public async Task RequireAsync(
            Context context,
            ActiveTenantContext activeContext,
            string permissionCode,
            CancellationToken cancellationToken)
        {
            if (await CanAsync(context, activeContext, permissionCode, cancellationToken))
                return;

            throw new GraphQLException(
                ErrorBuilder.New()
                    .SetMessage("Você não possui permissão para executar esta operação.")
                    .SetCode("AUTH_NOT_AUTHORIZED")
                    .Build());
        }

        public Task RequireAdminAsync(ActiveTenantContext activeContext)
        {
            if (activeContext.IsAdmin)
                return Task.CompletedTask;

            throw new GraphQLException(
                ErrorBuilder.New()
                    .SetMessage("Somente Admin pode executar esta operação.")
                    .SetCode("AUTH_NOT_AUTHORIZED")
                    .Build());
        }

        public static bool CanAssignToPersona(string personaCode, string permissionCode)
        {
            return personaCode != PersonaCodes.Admin
                && AllowedPersonas(permissionCode).Contains(personaCode);
        }

        private static string[] AllowedPersonas(string permissionCode) => permissionCode switch
        {
            PermissionCodes.CompanyEdit => [PersonaCodes.Operator, PersonaCodes.Company],
            PermissionCodes.CompanyCreate or PermissionCodes.CompanyManage
                or PermissionCodes.CollaboratorView or PermissionCodes.CollaboratorManage
                or PermissionCodes.SectorManage or PermissionCodes.CategoryManage
                or PermissionCodes.StandardSurveyCreate or PermissionCodes.StandardSurveyEdit
                or PermissionCodes.StandardSurveyPublish or PermissionCodes.StandardSurveyManage
                => [PersonaCodes.Operator],
            PermissionCodes.SurveyRespond => [PersonaCodes.Collaborator],
            PermissionCodes.SurveyTrack or PermissionCodes.CompanyView
                => [PersonaCodes.Operator, PersonaCodes.Company],
            PermissionCodes.TenantSwitch
                => [PersonaCodes.Operator, PersonaCodes.Collaborator, PersonaCodes.Company],
            _ => []
        };
    }
}
