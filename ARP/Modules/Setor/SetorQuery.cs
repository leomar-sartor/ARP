using ARP.Infra;
using ARP.Modules.Setor.Loaders;
using HotChocolate.Language;
using HotChocolate.Resolvers;

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
            [Service] Context context,
            IResolverContext resolverContext
            )
        {
            // Lê os argumentos brutos que chegaram na request
            var filterArg = resolverContext.ArgumentLiteral<IValueNode>("where");
            var sortArg = resolverContext.ArgumentLiteral<IValueNode>("order");
            var firstArg = resolverContext.ArgumentValue<int?>("first");
            var afterArg = resolverContext.ArgumentValue<string?>("after");

            //ou

            // ← Breakpoint aqui
            // No Watch Window adicione:
            // resolverContext.Selection.Arguments
            // resolverContext.Variables

            _logger.LogInformation(
            "GetSetores → where: {Filter} | order: {Sort} | first: {First} | after: {After}",
            filterArg, sortArg, firstArg, afterArg);

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
