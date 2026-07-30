using ARP.Infra;
using Microsoft.EntityFrameworkCore;

namespace ARP.Modules.Categoria.Loaders
{
    public class CategoriaByIdDataLoader : BatchDataLoader<long, Entity.Pesquisas.Categoria>
    {
        private readonly IDbContextFactory<Context> _contextFactory;

        public CategoriaByIdDataLoader(
            IBatchScheduler batchScheduler,
            IDbContextFactory<Context> contextFactory,
            DataLoaderOptions? options = null)
            : base(batchScheduler, options ?? new DataLoaderOptions())
        {
            _contextFactory = contextFactory;
        }

        /// <summary>
        /// Loads categorias by id in a single batch.
        /// </summary>
        protected override async Task<IReadOnlyDictionary<long, Entity.Pesquisas.Categoria>> LoadBatchAsync(
            IReadOnlyList<long> keys,
            CancellationToken cancellationToken)
        {
            await using var context = await _contextFactory.CreateDbContextAsync(cancellationToken);

            return await context.Categorias
                .Where(c => keys.Contains(c.Id))
                .ToDictionaryAsync(c => c.Id, cancellationToken);
        }
    }
}
