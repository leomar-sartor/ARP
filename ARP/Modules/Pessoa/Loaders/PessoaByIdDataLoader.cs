using ARP.Infra;
using Microsoft.EntityFrameworkCore;

namespace ARP.Modules.Pessoa.Loaders
{
    public class PessoaByIdDataLoader : BatchDataLoader<long, Entity.Pessoa>
    {
        private readonly IDbContextFactory<Context> _contextFactory;

        public PessoaByIdDataLoader(
            IBatchScheduler batchScheduler,
            IDbContextFactory<Context> contextFactory,
            DataLoaderOptions? options = null)
            : base(batchScheduler, options ?? new DataLoaderOptions())
        {
            _contextFactory = contextFactory;
        }

        protected override async Task<IReadOnlyDictionary<long, Entity.Pessoa>> LoadBatchAsync(
            IReadOnlyList<long> keys,
            CancellationToken cancellationToken)
        {
            await using var context = await _contextFactory.CreateDbContextAsync(cancellationToken);

            return await context.Pessoas
                .Where(p => keys.Contains(p.Id))
                .ToDictionaryAsync(p => p.Id, cancellationToken);
        }
    }
}