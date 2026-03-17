using ARP.Infra;
using ARP.Modules.Pessoa.Loaders;
using Microsoft.EntityFrameworkCore;

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

        [GraphQLDescription("Buscar pessoas com opções de paginação, filtragem, projections e ordenações - LIST")]
        [UsePaging(IncludeTotalCount = true)]
        [UseProjection]
        [UseFiltering]
        [UseSorting]
        public async Task<List<Entity.Pessoa>> GetPessoasAsync(
            [Service] Context context)
        {
            _logger.Log(LogLevel.Critical, "########################################");
            _logger.Log(LogLevel.Critical, "############## START SQL ###############");

            var result = await context.Pessoas
                //.Include(p => p.Enderecos)
                // -> Isso não precisa quando temos o resolver de endereços,
                // pois o HotChocolate irá otimizar a consulta e buscar os endereços apenas quando necessário
                .ToListAsync();

            _logger.Log(LogLevel.Critical, "############### END  SQL ###############");
            _logger.Log(LogLevel.Critical, "########################################");

            return result;
        }

        [GraphQLDescription("Buscar pessoas com opções de paginação, filtragem, projections e ordenações")]
        [UsePaging(IncludeTotalCount = true)]
        [UseProjection]
        [UseFiltering]
        [UseSorting]
        public async Task<IQueryable<Entity.Pessoa>> GetPessoasQueriableAsync(
            [Service] Context context)
        {
            //_logger.Log(LogLevel.Information, "Exemplo Information"); -- VERDE
            //_logger.Log(LogLevel.Warning, "Exemplo Warning"); -- AMARELO
            //_logger.Log(LogLevel.Error, "Exemplo Error"); -- VERMELHO com PRETO
            //_logger.Log(LogLevel.Critical, "Exemplo Error"); -- VERMELHO com BRANCO

            _logger.Log(LogLevel.Critical, "########################################");
            _logger.Log(LogLevel.Critical, "############## START SQL ###############");

            var result = context.Pessoas
                .Include(p => p.Enderecos)
                .AsQueryable();

            // Esse log vai aparecer somente quando a consulta for realmente executada, ou seja,
            // quando o HotChocolate precisar dos dados para resolver a query.
            // Isso é uma das vantagens de usar IQueryable,
            // pois permite que o HotChocolate otimize a consulta e busque apenas os dados necessários.

            _logger.Log(LogLevel.Critical, "############### END  SQL ###############");
            _logger.Log(LogLevel.Critical, "########################################");

            return result;
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
