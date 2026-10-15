using ARP.Entity.Cadastros;
using ARP.Entity.Enums;
using ARP.Infra;
using ARP.Modules.Auth;
using Microsoft.EntityFrameworkCore;

namespace ARP.Modules.Pesquisa
{
    public sealed record PesquisaOperationsSummary(
        long PesquisaId,
        string Nome,
        DateTime? DataInicial,
        DateTime? DataFinal,
        int Distribuidas,
        int Iniciadas,
        int Concluidas);

    [ExtendObjectType("Query")]
    public sealed class PesquisaOperationsQuery
    {
        [GraphQLDescription("Indicadores agregados de pesquisas no contexto ativo, sem respostas ou dados dos respondentes")]
        public async Task<IReadOnlyList<PesquisaOperationsSummary>> GetPesquisaOperationsSummary(
            [Service] Context context,
            [Service] TenantContextResolver tenantContextResolver,
            [Service] PermissionAuthorizationService permissionAuthorizationService,
            CancellationToken cancellationToken)
        {
            var activeContext = await tenantContextResolver.ResolveAsync(context, cancellationToken);
            await permissionAuthorizationService.RequireAsync(
                context, activeContext, PermissionCodes.SurveyTrack, cancellationToken);

            var surveys = context.Pesquisas
                .Where(survey => survey.Tipo == TipoPesquisa.Padronizada);
            var authorizedCompanyIds = PermissionAuthorizationService.AuthorizedCompanyIds(
                context.Empresas, context.UsuariosEmpresas, context.UsuariosPersonas,
                activeContext, PermissionCodes.SurveyTrack);

            if (activeContext.TenantId is long tenantId)
                surveys = surveys.Where(survey => survey.TenantId == tenantId);
            if (activeContext.CompanyId is long companyId)
                surveys = surveys.Where(survey => survey.EmpresasVinculadas.Any(link => link.EmpresaId == companyId));
            else if (!activeContext.IsAdmin)
                surveys = surveys.Where(survey => survey.EmpresasVinculadas.Any(link =>
                    authorizedCompanyIds.Contains(link.EmpresaId)));

            var surveyRows = await surveys
                .OrderBy(survey => survey.Nome)
                .Select(survey => new
                {
                    survey.Id,
                    survey.Nome,
                    survey.DataInicial,
                    survey.DataFinal
                })
                .ToListAsync(cancellationToken);

            if (surveyRows.Count == 0)
                return Array.Empty<PesquisaOperationsSummary>();

            var surveyIds = surveyRows.Select(survey => survey.Id).ToArray();
            var invitations = context.Convites
                .Where(invitation => surveyIds.Contains(invitation.PesquisaId)
                    && invitation.EmpresaId != null);

            if (activeContext.CompanyId is long selectedCompanyId)
                invitations = invitations.Where(invitation => invitation.EmpresaId == selectedCompanyId);
            else if (!activeContext.IsAdmin)
                invitations = invitations.Where(invitation => invitation.EmpresaId.HasValue
                    && authorizedCompanyIds.Contains(invitation.EmpresaId.Value));

            var statusRows = await invitations
                .GroupBy(invitation => invitation.PesquisaId)
                .Select(group => new
                {
                    PesquisaId = group.Key,
                    Distribuidas = group.Count(),
                    Iniciadas = group.Count(invitation => invitation.Status == Status.EmProgresso
                        || invitation.Status == Status.Completo),
                    Concluidas = group.Count(invitation => invitation.Status == Status.Completo)
                })
                .ToDictionaryAsync(row => row.PesquisaId, cancellationToken);

            return surveyRows.Select(survey =>
            {
                statusRows.TryGetValue(survey.Id, out var counts);
                return new PesquisaOperationsSummary(
                    survey.Id,
                    survey.Nome,
                    survey.DataInicial,
                    survey.DataFinal,
                    counts?.Distribuidas ?? 0,
                    counts?.Iniciadas ?? 0,
                    counts?.Concluidas ?? 0);
            }).ToArray();
        }
    }
}
