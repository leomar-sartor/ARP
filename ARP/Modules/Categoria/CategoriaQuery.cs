using ARP.Infra;
using ARP.Entity.Cadastros;
using ARP.Modules.Auth;
using ARP.Modules.Categoria.Loaders;

namespace ARP.Modules.Categoria
{
    [ExtendObjectType("Query")]
    public class CategoriaQuery
    {
        private readonly ILogger<CategoriaQuery> _logger;

        public CategoriaQuery(ILogger<CategoriaQuery> logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// Lists categorias with paging, filtering, projection and sorting.
        /// </summary>
        [GraphQLDescription("Buscar categorias com paginação, filtragem, projections e ordenações")]
        [UsePaging(IncludeTotalCount = true)]
        [UseProjection]
        [UseFiltering]
        [UseSorting]
        public async Task<IQueryable<Entity.Pesquisas.Categoria>> GetCategorias(
            [Service] Context context,
            [Service] TenantContextResolver tenantContextResolver,
            [Service] PermissionAuthorizationService permissionAuthorizationService,
            CancellationToken cancellationToken)
        {
            _logger.LogInformation("Buscando categorias");
            var activeContext = await tenantContextResolver.ResolveAsync(context, cancellationToken);
            await permissionAuthorizationService.RequireAsync(
                context, activeContext, PermissionCodes.CategoryManage, cancellationToken);
            return context.Categorias.AsQueryable();
        }

        /// <summary>
        /// Returns a categoria by id.
        /// </summary>
        [GraphQLDescription("Buscar categoria por id")]
        public async Task<Entity.Pesquisas.Categoria?> GetCategoriaById(
            long id,
            [Service] Context context,
            [Service] TenantContextResolver tenantContextResolver,
            [Service] PermissionAuthorizationService permissionAuthorizationService,
            CategoriaByIdDataLoader dataLoader,
            CancellationToken cancellationToken)
        {
            var activeContext = await tenantContextResolver.ResolveAsync(context, cancellationToken);
            await permissionAuthorizationService.RequireAsync(
                context, activeContext, PermissionCodes.CategoryManage, cancellationToken);
            return await dataLoader.LoadAsync(id, cancellationToken);
        }
    }
}
