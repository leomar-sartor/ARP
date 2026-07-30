using ARP.Infra;
using ARP.Modules.Pesquisa.Loaders;
using ARP.Modules.Pesquisa.Types;
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

        [GraphQLDescription("Buscar pesquisas com opções de paginação, filtragem, projections e ordenações")]
        [UsePaging(IncludeTotalCount = true)]
        [UseProjection]
        [UseFiltering]
        [UseSorting]
        public async Task<IQueryable<Entity.Pesquisas.Pesquisa>> GetPesquisasAsync(
            [Service] Context context)
        {
            var result = context.Pesquisas
                .Include(p => p.Questoes)
                .Include(p => p.Convites)
                .AsQueryable();

            return result;
        }

        /// <summary>
        /// Returns a pesquisa by id with nested questoes and opcoes for the edit form.
        /// </summary>
        [GraphQLDescription("Buscar pesquisa por id")]
        public async Task<Entity.Pesquisas.Pesquisa?> GetPesquisaById(
            long id,
            PesquisaByIdDataLoader dataLoader,
            CancellationToken cancellationToken)
        {
            return await dataLoader.LoadAsync(id, cancellationToken);
        }

        // Retomar ou iniciar pesquisa pelo token
        [GraphQLDescription("Buscar progresso da pesquisa pelo token do convite")]
        public async Task<PesquisaSessaoPayload> GetSessaoPesquisa(
            string token,
            [Service] Context context,
            CancellationToken ct)
        {
            try
            {
                var convite = await context.Convites
                    .Include(c => c.Pesquisa).ThenInclude(p => p.Questoes).ThenInclude(q => q.Opcoes)
                    .FirstOrDefaultAsync(c => c.Token == token, ct)
                    ?? throw new ArgumentException("Token inválido.");

                if (convite.Status == Entity.Enums.Status.Completo)
                    throw new ArgumentException("Pesquisa já concluída.");

                var rascunho = await context.PesquisaRascunhos
                    .FirstOrDefaultAsync(r => r.Token == token, ct);

                return new PesquisaSessaoPayload(
                    Pesquisa: convite.Pesquisa,
                    UltimaQuestaoRespondidaId: rascunho?.UltimaQuestaoRespondidaId,
                    RespostasParciais: rascunho?.RespostasParciais
                );
            }
            catch
            {
                return new PesquisaSessaoPayload(
                    Pesquisa: new Entity.Pesquisas.Pesquisa(),
                    UltimaQuestaoRespondidaId: null,
                    RespostasParciais: null
                );
            }


        }
    }
}
