using ARP.Infra;
using ARP.Entity.Cadastros;
using ARP.Modules.Auth;
using ARP.Modules.Colaborador.Loaders;
using Microsoft.EntityFrameworkCore;

namespace ARP.Modules.Colaborador
{
    [ExtendObjectType("Query")]
    public class ColaboradorQuery
    {
        private readonly ILogger<ColaboradorQuery> _logger;

        public ColaboradorQuery(
            ILogger<ColaboradorQuery> logger
            )
        {
            _logger = logger;
        }

        [GraphQLDescription("Buscar colaboradores com opções de paginação, filtragem, projections e ordenações")]
        [UsePaging(IncludeTotalCount = true)]
        [UseProjection]
        [UseFiltering]
        [UseSorting]
        public async Task<IQueryable<Entity.Cadastros.Colaborador>> ColaboradoresAsync(
            [Service] Context context,
            [Service] TenantContextResolver tenantContextResolver,
            [Service] PermissionAuthorizationService permissionAuthorizationService,
            CancellationToken cancellationToken)
        {
            var activeContext = await tenantContextResolver.ResolveAsync(context, cancellationToken);
            await permissionAuthorizationService.RequireAsync(
                context, activeContext, PermissionCodes.CollaboratorView, cancellationToken);

            var result = context.Colaboradores
                .AsQueryable();

            if (!activeContext.IsAdmin && activeContext.TenantId is long tenantId)
                result = result.Where(c => context.ColaboradorEmpresas.Any(link =>
                    link.ColaboradorId == c.Id
                    && link.Ativo
                    && link.Empresa.TenantId == tenantId
                    && (activeContext.IsAdmin || context.UsuariosEmpresas.Any(m =>
                        m.UsuarioId == activeContext.UserId
                        && m.EmpresaId == link.EmpresaId
                        && m.Ativo))));
            if (!activeContext.IsAdmin && activeContext.CompanyId is long companyId)
                result = result.Where(c => context.ColaboradorEmpresas.Any(link =>
                    link.ColaboradorId == c.Id && link.EmpresaId == companyId && link.Ativo));

            return result;
        }

        [GraphQLDescription("Buscar por colaborador")]
        public async Task<Entity.Cadastros.Colaborador?> GetColaboradorById(
        long id,
        [Service] Context context,
        [Service] TenantContextResolver tenantContextResolver,
        [Service] PermissionAuthorizationService permissionAuthorizationService,
        ColaboradorByIdDataLoader dataLoader,
        CancellationToken cancellationToken)
        {
            var activeContext = await tenantContextResolver.ResolveAsync(context, cancellationToken);
            await permissionAuthorizationService.RequireAsync(
                context, activeContext, PermissionCodes.CollaboratorView, cancellationToken);
            return await dataLoader.LoadAsync(id, cancellationToken);
        }
    }
}
