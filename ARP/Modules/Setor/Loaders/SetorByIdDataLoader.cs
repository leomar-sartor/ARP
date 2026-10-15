using ARP.Infra;
using ARP.Modules.Auth;
using Microsoft.EntityFrameworkCore;

namespace ARP.Modules.Setor.Loaders
{
    public class SetorByIdDataLoader : BatchDataLoader<long, Entity.Cadastros.Setor>
    {
        private readonly IDbContextFactory<Context> _contextFactory;
        private readonly TenantContextResolver _tenantContextResolver;

        public SetorByIdDataLoader(
            IBatchScheduler batchScheduler,
            IDbContextFactory<Context> contextFactory,
            TenantContextResolver tenantContextResolver,
            DataLoaderOptions? options = null)
            : base(batchScheduler, options ?? new DataLoaderOptions())
        {
            _contextFactory = contextFactory;
            _tenantContextResolver = tenantContextResolver;
        }

        protected override async Task<IReadOnlyDictionary<long, Entity.Cadastros.Setor>> LoadBatchAsync(
            IReadOnlyList<long> keys,
            CancellationToken cancellationToken)
        {
            await using var context = await _contextFactory.CreateDbContextAsync(cancellationToken);

            var activeContext = await _tenantContextResolver.ResolveAsync(context, cancellationToken);
            var query = context.Setores
                .Include(s => s.Empresa)
                .Where(p => keys.Contains(p.Id));
            if (!activeContext.IsAdmin && activeContext.TenantId is long tenantId)
                query = query.Where(s => s.Empresa.TenantId == tenantId);
            if (!activeContext.IsAdmin && activeContext.CompanyId is long companyId)
                query = query.Where(s => s.EmpresaId == companyId);
            if (!activeContext.IsAdmin)
                query = query.Where(s => context.UsuariosEmpresas.Any(m =>
                    m.UsuarioId == activeContext.UserId && m.EmpresaId == s.EmpresaId && m.Ativo));

            return await query.ToDictionaryAsync(p => p.Id, cancellationToken);
        }
    }
}
