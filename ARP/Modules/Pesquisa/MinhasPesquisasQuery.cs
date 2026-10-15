using ARP.Entity.Enums;
using ARP.Infra;
using ARP.Modules.Auth;
using Microsoft.EntityFrameworkCore;

namespace ARP.Modules.Pesquisa;

public sealed record MinhaParticipacaoPesquisa(long Id, string Nome, Status Status, string? Token);

[ExtendObjectType("Query")]
public sealed class MinhasPesquisasQuery
{
    public async Task<IReadOnlyList<MinhaParticipacaoPesquisa>> GetMinhasPesquisas(
        [Service] Context context,
        [Service] TenantContextResolver tenantContextResolver,
        [Service] PermissionAuthorizationService permissionAuthorizationService,
        CancellationToken cancellationToken)
    {
        var activeContext = await tenantContextResolver.ResolveAsync(context, cancellationToken);
        await permissionAuthorizationService.RequireAsync(context, activeContext,
            ARP.Entity.Cadastros.PermissionCodes.SurveyRespond, cancellationToken);
        var companyId = activeContext.CompanyId
            ?? throw new GraphQLException("Selecione uma empresa para consultar suas pesquisas.");
        return await context.Convites
            .Where(invitation => invitation.EmpresaId == companyId
                && invitation.Pesquisa.TenantId == activeContext.TenantId
                && invitation.Colaborador.UsuarioId == activeContext.UserId
                && invitation.Colaborador.Ativo
                && context.ColaboradorEmpresas.Any(link => link.ColaboradorId == invitation.ColaboradorId
                    && link.EmpresaId == companyId && link.Ativo))
            .OrderByDescending(invitation => invitation.EnviadoEm)
            .Select(invitation => new MinhaParticipacaoPesquisa(invitation.Id, invitation.Pesquisa.Nome,
                invitation.Status, invitation.Status == Status.Completo ? null : invitation.Token))
            .ToArrayAsync(cancellationToken);
    }
}
