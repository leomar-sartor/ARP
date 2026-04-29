using ARP.Infra;
using Microsoft.EntityFrameworkCore;

namespace ARP.Modules.Pesquisa
{
    [ExtendObjectType("Query")]
    public class PesquisaQuery
    {
        private readonly ILogger<PesquisaQuery> _logger;

        public PesquisaQuery(
            ILogger<PesquisaQuery> logger
            )
        {
            _logger = logger;
        }

        [GraphQLDescription("Buscar pessoas com opções de paginação, filtragem, projections e ordenações")]
        [UsePaging(IncludeTotalCount = true)]
        [UseProjection]
        [UseFiltering]
        [UseSorting]
        public async Task<IQueryable<Entity.Pesquisa>> GetPesquisasAsync(
            [Service] Context context)
        {
            var result = context.Pesquisas
                .Include(p => p.Questoes)
                .Include(p => p.Convites)
                .AsQueryable();

            return result;
        }
    }
}
