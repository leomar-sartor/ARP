using ARP.Infra;
using Microsoft.EntityFrameworkCore;

namespace ARP.Modules.Colaborador.Loaders
{
    public class ColaboradorByIdDataLoader : BatchDataLoader<long, Entity.Colaborador>
    {
        private readonly IDbContextFactory<Context> _contextFactory;

        public ColaboradorByIdDataLoader(
            IBatchScheduler batchScheduler,
            IDbContextFactory<Context> contextFactory,
            DataLoaderOptions? options = null)
            : base(batchScheduler, options ?? new DataLoaderOptions())
        {
            _contextFactory = contextFactory;
        }

        protected override async Task<IReadOnlyDictionary<long, Entity.Colaborador>> LoadBatchAsync(
           IReadOnlyList<long> keys,
           CancellationToken cancellationToken)
        {
            await using var context = await _contextFactory.CreateDbContextAsync(cancellationToken);

            return await context.Colaboradores
                .Where(p => keys.Contains(p.Id))
                .ToDictionaryAsync(p => p.Id, cancellationToken);
        }
    }
}
