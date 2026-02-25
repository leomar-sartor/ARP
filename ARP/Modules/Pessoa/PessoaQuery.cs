using ARP.Infra;
using ARP.Modules.Pessoa.DataLoader;

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

        }

        [GraphQLDescription("Buscar Pessoas com Paginação, Filtragem, Projections e Ordenação")]
        [UsePaging(IncludeTotalCount = true)]
        [UseProjection]
        [UseFiltering]
        [UseSorting]
        public IQueryable<Entity.Pessoa> GetPessoas(
            [Service] Context context)
            => context.Pessoas.AsQueryable();

        public async Task<Entity.Pessoa?> GetPessoaById(
        long id,
        PessoaByIdDataLoader dataLoader,
        CancellationToken cancellationToken)
            {
                return await dataLoader.LoadAsync(id, cancellationToken);
            }
    }
}
