using ARP.Infra;
using ARP.Entity.Cadastros;
using ARP.Modules.Auth;
using ARP.Modules.Setor.Loaders;
using Microsoft.EntityFrameworkCore;



namespace ARP.Modules.Setor
{
    [ExtendObjectType("Query")]
    public class SetorQuery
    {
        private readonly ILogger<SetorQuery> _logger;

        public SetorQuery(
            ILogger<SetorQuery> logger
            )
        {
            _logger = logger;
        }

        [GraphQLDescription("Buscar setores com opções de paginação, filtragem, projections e ordenações")]
        [UsePaging(IncludeTotalCount = true)]
        [UseProjection]
        [UseFiltering]
        [UseSorting]
        public async Task<IQueryable<Entity.Cadastros.Setor>> GetSetores(
            [Service] Context context,

            [Service] TenantContextResolver tenantContextResolver,
            [Service] PermissionAuthorizationService permissionAuthorizationService,
            CancellationToken cancellationToken
            )
        {
            var activeContext = await tenantContextResolver.ResolveAsync(context, cancellationToken);
            await permissionAuthorizationService.RequireAsync(
                context, activeContext, PermissionCodes.SectorManage, cancellationToken);

            _logger.Log(LogLevel.Information, "Buscando setores");

            var setores = context.Setores.AsQueryable();
            if (!activeContext.IsAdmin && activeContext.TenantId is long tenantId)
                setores = setores.Where(s => s.Empresa.TenantId == tenantId);
            if (!activeContext.IsAdmin && activeContext.CompanyId is long companyId)
                setores = setores.Where(s => s.EmpresaId == companyId);
            if (!activeContext.IsAdmin)
                setores = setores.Where(s => context.UsuariosEmpresas.Any(m =>
                    m.UsuarioId == activeContext.UserId && m.EmpresaId == s.EmpresaId && m.Ativo));

            return setores;
        }

        [GraphQLDescription("Buscar por setor")]
        public async Task<Entity.Cadastros.Setor?> GetSetorById(
        long id,
        SetorByIdDataLoader dataLoader,
        [Service] Context context,
        [Service] TenantContextResolver tenantContextResolver,
        [Service] PermissionAuthorizationService permissionAuthorizationService,
        CancellationToken cancellationToken)
        {
            var activeContext = await tenantContextResolver.ResolveAsync(context, cancellationToken);
            await permissionAuthorizationService.RequireAsync(
                context, activeContext, PermissionCodes.SectorManage, cancellationToken);
            return await dataLoader.LoadAsync(id, cancellationToken);
        }
    }
}
