using ARP.Entity;
using ARP.Infra;
using Microsoft.EntityFrameworkCore;

namespace ARP.Modules.Empresa.Loaders
{
    public class SetoresByEmpresaIdDataLoader
    : BatchDataLoader<long, IReadOnlyList<Setor>>
    {
        private readonly IDbContextFactory<Context> _factory;

        public SetoresByEmpresaIdDataLoader(
            IBatchScheduler scheduler,
            IDbContextFactory<Context> factory,
            DataLoaderOptions? options = null)
            : base(scheduler, options)
        {
            _factory = factory;
        }

        protected override async Task<IReadOnlyDictionary<long, IReadOnlyList<Setor>>>
            LoadBatchAsync(
                IReadOnlyList<long> keys,
                CancellationToken cancellationToken)
        {
            await using var context =
                await _factory.CreateDbContextAsync(cancellationToken);

            //var data = await context.EmpresaSetores
            //    .Where(es => keys.Contains(es.EmpresaId))
            //    .Include(es => es.Setor)
            //    .ToListAsync(cancellationToken);

            //Projections
            var data = await context.EmpresaSetores
                .Where(es => keys.Contains(es.EmpresaId))
                .Select(es => new
                {
                    es.EmpresaId,
                    es.Setor
                })
                .ToListAsync(cancellationToken);

            return data
                .GroupBy(es => es.EmpresaId)
                .ToDictionary(
                    g => g.Key,
                    g => (IReadOnlyList<Setor>)g
                            .Select(es => es.Setor)
                            .ToList());
        }
    }
}
