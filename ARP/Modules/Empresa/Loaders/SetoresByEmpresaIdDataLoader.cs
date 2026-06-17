using ARP.Infra;
using Microsoft.EntityFrameworkCore;

namespace ARP.Modules.Empresa.Loaders
{
    public class SetoresByEmpresaIdDataLoader
    : BatchDataLoader<long, IReadOnlyList<Entity.Cadastros.Setor>>
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

        protected override async Task<IReadOnlyDictionary<long, IReadOnlyList<Entity.Cadastros.Setor>>>
            LoadBatchAsync(
                IReadOnlyList<long> keys,
                CancellationToken cancellationToken)
        {
            await using var context =
                await _factory.CreateDbContextAsync(cancellationToken);

            //Projections
            var setores = await context.Setores
                .Where(s => keys.Contains(s.EmpresaId))
                .ToListAsync(cancellationToken);

            return setores
               .GroupBy(e => e.EmpresaId)
               .ToDictionary(
                   g => g.Key,
                   g => (IReadOnlyList<Entity.Cadastros.Setor>)g.ToList());
        }
    }
}
