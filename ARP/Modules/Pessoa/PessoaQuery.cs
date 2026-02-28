using ARP.Infra;
using ARP.Modules.Pessoa.Loaders;

namespace ARP.Modules.Pessoa
{
    [ExtendObjectType("Query")]
    public class PessoaQuery
    {
        private readonly ILogger<PessoaQuery> _logger;

        public PessoaQuery(
            ILogger<PessoaQuery> logger
            )
        {
            _logger = logger;
        }

        [GraphQLDescription("Buscar pessoas com opções de paginação, filtragem, projections e ordenações")]
        [UsePaging(IncludeTotalCount = true)]
        [UseProjection]
        [UseFiltering]
        [UseSorting]
        public IQueryable<Entity.Pessoa> GetPessoas(
            [Service] Context context)
        {
            _logger.Log(LogLevel.Information, "Exemplo Information");
            _logger.Log(LogLevel.Warning, "Exemplo Warning");
            _logger.Log(LogLevel.Error, "Exemplo Error");
            _logger.Log(LogLevel.Critical, "Exemplo Critical");

            return context.Pessoas.AsQueryable();
        }

        [GraphQLDescription("Buscar por pessoa")]
        public async Task<Entity.Pessoa?> GetPessoaById(
        long id,
        PessoaByIdDataLoader dataLoader,
        CancellationToken cancellationToken)
        {
            return await dataLoader.LoadAsync(id, cancellationToken);
        }
    }
}
