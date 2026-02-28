using ARP.Infra;
using ARP.Modules.Setor.Loaders;

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
        public IQueryable<Entity.Setor> GetSetores(
            [Service] Context context)
        {
            _logger.Log(LogLevel.Information, "Buscando setores");

            return context.Setores.AsQueryable();
        }

        [GraphQLDescription("Buscar por setor")]
        public async Task<Entity.Setor?> GetSetorById(
        long id,
        SetorByIdDataLoader dataLoader,
        CancellationToken cancellationToken)
        {
            return await dataLoader.LoadAsync(id, cancellationToken);
        }
    }
}
