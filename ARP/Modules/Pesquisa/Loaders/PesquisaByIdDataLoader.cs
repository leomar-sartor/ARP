using ARP.Infra;
using Microsoft.EntityFrameworkCore;

namespace ARP.Modules.Pesquisa.Loaders
{
    public class PesquisaByIdDataLoader : BatchDataLoader<long, Entity.Pesquisas.Pesquisa>
    {
        private readonly IDbContextFactory<Context> _contextFactory;

        public PesquisaByIdDataLoader(
            IBatchScheduler batchScheduler,
            IDbContextFactory<Context> contextFactory,
            DataLoaderOptions? options = null)
            : base(batchScheduler, options ?? new DataLoaderOptions())
        {
            _contextFactory = contextFactory;
        }

        /// <summary>
        /// Loads pesquisas by id including questoes and opcoes for edit forms.
        /// </summary>
        protected override async Task<IReadOnlyDictionary<long, Entity.Pesquisas.Pesquisa>> LoadBatchAsync(
            IReadOnlyList<long> keys,
            CancellationToken cancellationToken)
        {
            await using var context = await _contextFactory.CreateDbContextAsync(cancellationToken);

            var pesquisas = await context.Pesquisas
                .Include(p => p.Questoes)
                    .ThenInclude(q => q.Opcoes)
                .Include(p => p.Questoes)
                    .ThenInclude(q => q.Categoria)
                .Where(p => keys.Contains(p.Id))
                .ToListAsync(cancellationToken);

            foreach (var pesquisa in pesquisas)
            {
                pesquisa.Questoes = pesquisa.Questoes
                    .OrderBy(q => q.Ordem)
                    .ThenBy(q => q.Id)
                    .ToList();

                foreach (var questao in pesquisa.Questoes)
                {
                    questao.Opcoes = questao.Opcoes
                        .OrderBy(o => o.Ordem)
                        .ThenBy(o => o.Id)
                        .ToList();
                }
            }

            return pesquisas.ToDictionary(p => p.Id);
        }
    }
}
