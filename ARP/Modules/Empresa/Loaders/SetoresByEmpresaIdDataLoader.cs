using ARP.Infra;
using Microsoft.EntityFrameworkCore;

namespace ARP.Modules.Empresa.Loaders
{
    public class SetoresByEmpresaIdDataLoader
    : BatchDataLoader<long, IReadOnlyList<Entity.Setor>>
    {
        private readonly IDbContextFactory<Context> _factory;

        public SetoresByEmpresaIdDataLoader(
            IBatchScheduler scheduler,
            IDbContextFactory<Context> factory,
            DataLoaderOptions? options = null)
            : base(scheduler, options ?? new DataLoaderOptions())
        {
            _factory = factory;
        }

        protected override async Task<IReadOnlyDictionary<long, IReadOnlyList<Entity.Setor>>>
            LoadBatchAsync(
                IReadOnlyList<long> keys,
                CancellationToken cancellationToken)
        {
            await using var context =
                await _factory.CreateDbContextAsync(cancellationToken);

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
                    g => (IReadOnlyList<Entity.Setor>)g
                            .Select(es => es.Setor)
                            .ToList());
        }
    }
}
