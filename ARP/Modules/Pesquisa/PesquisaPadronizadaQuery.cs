using ARP.Entity.Cadastros;
using ARP.Entity.Enums;
using ARP.Infra;
using ARP.Modules.Auth;
using ARP.Modules.Pesquisa.Types;
using Microsoft.EntityFrameworkCore;

namespace ARP.Modules.Pesquisa
{
    [ExtendObjectType("Query")]
    public sealed class PesquisaPadronizadaQuery
    {
        [GraphQLDescription("Listar pesquisas padronizadas disponíveis no tenant ativo")]
        [UsePaging(IncludeTotalCount = true)]
        [UseFiltering]
        [UseSorting]
        public async Task<IQueryable<PesquisaPadronizadaResumo>> GetPesquisasPadronizadas(
            [Service] Context context,
            [Service] TenantContextResolver tenantContextResolver,
            [Service] PermissionAuthorizationService permissionAuthorizationService,
            CancellationToken cancellationToken)
        {
            var activeContext = await tenantContextResolver.ResolveAsync(context, cancellationToken);
            await permissionAuthorizationService.RequireAsync(
                context, activeContext, PermissionCodes.StandardSurveyManage, cancellationToken);

            if (activeContext.TenantId is not long tenantId)
                throw new GraphQLException("Selecione um tenant para listar pesquisas padronizadas.");

            var surveys = context.Pesquisas
                .Where(survey => survey.Tipo == TipoPesquisa.Padronizada
                    && survey.TenantId == tenantId);
            if (!activeContext.IsAdmin)
            {
                var companyIds = PermissionAuthorizationService.AuthorizedCompanyIds(
                    context.Empresas, context.UsuariosEmpresas, context.UsuariosPersonas,
                    activeContext, PermissionCodes.StandardSurveyManage);
                surveys = surveys.Where(survey => survey.EmpresasVinculadas.Any()
                    && survey.EmpresasVinculadas.All(link => companyIds.Contains(link.EmpresaId)));
                if (activeContext.CompanyId is long companyId)
                    surveys = surveys.Where(survey => survey.EmpresasVinculadas.Any(link => link.EmpresaId == companyId));
            }
            return surveys.Select(PesquisaPadronizadaResumo.Projection);
        }

        public async Task<PesquisaPadronizadaResumo?> GetPesquisaPadronizadaById(
            long id, [Service] Context context,
            [Service] TenantContextResolver tenantContextResolver,
            [Service] PermissionAuthorizationService permissionAuthorizationService,
            CancellationToken cancellationToken)
        {
            var surveys = await GetPesquisasPadronizadas(context, tenantContextResolver,
                permissionAuthorizationService, cancellationToken);
            return await surveys.FirstOrDefaultAsync(survey => survey.Id == id, cancellationToken);
        }
    }
}
