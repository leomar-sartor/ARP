using ARP.Infra;
using ARP.Modules.Auth;
using Microsoft.EntityFrameworkCore;

namespace ARP.Modules.Empresa.Loaders
{
    public class SetoresByEmpresaIdDataLoader
    : BatchDataLoader<long, IReadOnlyList<Entity.Cadastros.Setor>>
    {
        private readonly IDbContextFactory<Context> _factory;
        private readonly TenantContextResolver _tenantContextResolver;

        public SetoresByEmpresaIdDataLoader(
            IBatchScheduler scheduler,
            IDbContextFactory<Context> factory,
            TenantContextResolver tenantContextResolver,
            DataLoaderOptions? options = null)
            : base(scheduler, options ?? new DataLoaderOptions())
        {
            _factory = factory;
            _tenantContextResolver = tenantContextResolver;
        }

        protected override async Task<IReadOnlyDictionary<long, IReadOnlyList<Entity.Cadastros.Setor>>>
            LoadBatchAsync(
                IReadOnlyList<long> keys,
                CancellationToken cancellationToken)
        {
            await using var context =
                await _factory.CreateDbContextAsync(cancellationToken);

            var activeContext = await _tenantContextResolver.ResolveAsync(context, cancellationToken);
            var query = context.Setores.Where(s => keys.Contains(s.EmpresaId));
            if (activeContext.TenantId is long tenantId)
                query = query.Where(s => s.Empresa.TenantId == tenantId);
            if (activeContext.CompanyId is long companyId)
                query = query.Where(s => s.EmpresaId == companyId);
            if (!activeContext.IsAdmin)
                query = query.Where(s => context.UsuariosEmpresas.Any(m =>
                    m.UsuarioId == activeContext.UserId && m.EmpresaId == s.EmpresaId && m.Ativo));

            var setores = await query.ToListAsync(cancellationToken);

            return setores
               .GroupBy(e => e.EmpresaId)
               .ToDictionary(
                   g => g.Key,
                   g => (IReadOnlyList<Entity.Cadastros.Setor>)g.ToList());
        }
    }
}
