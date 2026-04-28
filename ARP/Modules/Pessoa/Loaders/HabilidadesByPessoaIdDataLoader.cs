using ARP.Entity;
using ARP.Infra;
using Microsoft.EntityFrameworkCore;

namespace ARP.Modules.Pessoa.Loaders
{
    public class HabilidadesByPessoaIdDataLoader 
        : BatchDataLoader<long, IReadOnlyList<Habilidade>>
    {
        private readonly IDbContextFactory<Context> _factory;

        public HabilidadesByPessoaIdDataLoader(
            IBatchScheduler scheduler,
            IDbContextFactory<Context> factory,
            DataLoaderOptions? options = null)
            : base(scheduler, options: options ?? new DataLoaderOptions())
        {
            _factory = factory;
        }

        protected override async Task<IReadOnlyDictionary<long, IReadOnlyList<Habilidade>>>
            LoadBatchAsync(
                IReadOnlyList<long> keys,
                CancellationToken ct)
        {
            await using var context = await _factory.CreateDbContextAsync(ct);

            var data = await context.PessoaHabilidades
                .Where(ph => keys.Contains(ph.PessoaId))
                .Select(ph => new
                {
                    ph.PessoaId,
                    ph.Habilidade
                })
                .ToListAsync(ct);

            return data
                .GroupBy(x => x.PessoaId)
                .ToDictionary(
                    g => g.Key,
                    g => (IReadOnlyList<Habilidade>)g.Select(x => x.Habilidade).ToList());
        }
    }
}
