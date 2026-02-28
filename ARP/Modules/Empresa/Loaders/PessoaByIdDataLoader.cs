using ARP.Infra;
using Microsoft.EntityFrameworkCore;

namespace ARP.Modules.Empresa.Loaders
{
    public class EmpresaByIdDataLoader : BatchDataLoader<long, Entity.Empresa>
    {
        private readonly IDbContextFactory<Context> _contextFactory;

        public EmpresaByIdDataLoader(
            IBatchScheduler batchScheduler,
            IDbContextFactory<Context> contextFactory,
            DataLoaderOptions? options = null)
            : base(batchScheduler, options ?? new DataLoaderOptions())
        {
            _contextFactory = contextFactory;
        }

        protected override async Task<IReadOnlyDictionary<long, Entity.Empresa>> LoadBatchAsync(
            IReadOnlyList<long> keys,
            CancellationToken cancellationToken)
        {
            await using var context = await _contextFactory.CreateDbContextAsync(cancellationToken);

            return await context.Empresas
                .Where(p => keys.Contains(p.Id))
                .ToDictionaryAsync(p => p.Id, cancellationToken);
        }
    }
}