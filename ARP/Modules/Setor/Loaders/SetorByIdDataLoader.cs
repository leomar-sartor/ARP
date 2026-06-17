using ARP.Infra;
using Microsoft.EntityFrameworkCore;

namespace ARP.Modules.Setor.Loaders
{
    public class SetorByIdDataLoader : BatchDataLoader<long, Entity.Cadastros.Setor>
    {
        private readonly IDbContextFactory<Context> _contextFactory;

        public SetorByIdDataLoader(
            IBatchScheduler batchScheduler,
            IDbContextFactory<Context> contextFactory,
            DataLoaderOptions? options = null)
            : base(batchScheduler, options ?? new DataLoaderOptions())
        {
            _contextFactory = contextFactory;
        }

        protected override async Task<IReadOnlyDictionary<long, Entity.Cadastros.Setor>> LoadBatchAsync(
            IReadOnlyList<long> keys,
            CancellationToken cancellationToken)
        {
            await using var context = await _contextFactory.CreateDbContextAsync(cancellationToken);

            return await context.Setores
                .Include(s => s.Empresa)
                .Where(p => keys.Contains(p.Id))
                .ToDictionaryAsync(p => p.Id, cancellationToken);
        }
    }
}
