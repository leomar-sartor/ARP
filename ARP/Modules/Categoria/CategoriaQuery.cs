using ARP.Infra;
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
        public IQueryable<Entity.Pesquisas.Categoria> GetCategorias([Service] Context context)
        {
            _logger.LogInformation("Buscando categorias");
            return context.Categorias.AsQueryable();
        }

        /// <summary>
        /// Returns a categoria by id.
        /// </summary>
        [GraphQLDescription("Buscar categoria por id")]
        public async Task<Entity.Pesquisas.Categoria?> GetCategoriaById(
            long id,
            CategoriaByIdDataLoader dataLoader,
            CancellationToken cancellationToken)
        {
            return await dataLoader.LoadAsync(id, cancellationToken);
        }
    }
}
