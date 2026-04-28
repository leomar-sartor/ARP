using ARP.Entity;
using ARP.Infra;
using Microsoft.EntityFrameworkCore;

namespace ARP.Modules.Pessoa.Loaders
{
    public class EnderecosByPessoaIdDataLoader
        : BatchDataLoader<long, IReadOnlyList<Endereco>>
    {
        private readonly IDbContextFactory<Context> _factory;

        public EnderecosByPessoaIdDataLoader(
            IBatchScheduler scheduler,
            IDbContextFactory<Context> factory,
            DataLoaderOptions? options = null)
            : base(scheduler, options: options ?? new DataLoaderOptions())
        {
            _factory = factory;
        }

        protected override async Task<IReadOnlyDictionary<long, IReadOnlyList<Endereco>>>
            LoadBatchAsync(
                IReadOnlyList<long> keys,
                CancellationToken cancellationToken)
        {
            await using var context =
                await _factory.CreateDbContextAsync(cancellationToken);

            var enderecos = await context.Enderecos
                .Where(e => keys.Contains(e.PessoaId) && e.DeletedAt == null)
                .ToListAsync(cancellationToken);

            return enderecos
                .GroupBy(e => e.PessoaId)
                .ToDictionary(
                    g => g.Key,
                    g => (IReadOnlyList<Endereco>)g.ToList());
        }
    }
}
