using ARP.Infra;
using ARP.Modules.Auth;
using Microsoft.EntityFrameworkCore;

namespace ARP.Modules.Empresa.Loaders
{
    public class EmpresaByIdDataLoader : BatchDataLoader<long, Entity.Cadastros.Empresa>
    {
        private readonly IDbContextFactory<Context> _contextFactory;
        private readonly TenantContextResolver _tenantContextResolver;

        public EmpresaByIdDataLoader(
            IBatchScheduler batchScheduler,
            IDbContextFactory<Context> contextFactory,
            TenantContextResolver tenantContextResolver,
            DataLoaderOptions? options = null)
            : base(batchScheduler, options ?? new DataLoaderOptions())
        {
            _contextFactory = contextFactory;
            _tenantContextResolver = tenantContextResolver;
        }

        protected override async Task<IReadOnlyDictionary<long, Entity.Cadastros.Empresa>> LoadBatchAsync(
            IReadOnlyList<long> keys,
            CancellationToken cancellationToken)
        {
            await using var context = await _contextFactory.CreateDbContextAsync(cancellationToken);

            var activeContext = await _tenantContextResolver.ResolveAsync(context, cancellationToken);
            var empresas = context.Empresas.Where(p => keys.Contains(p.Id));
            if (!activeContext.IsAdmin && activeContext.TenantId is long tenantId)
                empresas = empresas.Where(e => e.TenantId == tenantId);
            if (!activeContext.IsAdmin && activeContext.CompanyId is long companyId)
                empresas = empresas.Where(e => e.Id == companyId);
            if (!activeContext.IsAdmin)
                empresas = empresas.Where(e => context.UsuariosEmpresas.Any(m =>
                    m.UsuarioId == activeContext.UserId && m.EmpresaId == e.Id && m.Ativo));

            return await empresas
                .ToDictionaryAsync(p => p.Id, cancellationToken);
        }
    }
}