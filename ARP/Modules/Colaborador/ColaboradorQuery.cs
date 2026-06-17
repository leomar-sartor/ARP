using ARP.Infra;
using ARP.Modules.Colaborador.Loaders;

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
            [Service] Context context)
        {
            var result = context.Colaboradores
                .AsQueryable();

            return result;
        }

        [GraphQLDescription("Buscar por colaborador")]
        public async Task<Entity.Cadastros.Colaborador?> GetColaboradorById(
        long id,
        ColaboradorByIdDataLoader dataLoader,
        CancellationToken cancellationToken)
        {
            return await dataLoader.LoadAsync(id, cancellationToken);
        }
    }
}
