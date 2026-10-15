using ARP.Infra;
using ARP.Modules.Auth;
using Microsoft.EntityFrameworkCore;

namespace ARP.Modules.Colaborador.Loaders
{
    public class ColaboradorByIdDataLoader : BatchDataLoader<long, Entity.Cadastros.Colaborador>
    {
        private readonly IDbContextFactory<Context> _contextFactory;
        private readonly TenantContextResolver _tenantContextResolver;

        public ColaboradorByIdDataLoader(
            IBatchScheduler batchScheduler,
            IDbContextFactory<Context> contextFactory,
            TenantContextResolver tenantContextResolver,
            DataLoaderOptions? options = null)
            : base(batchScheduler, options ?? new DataLoaderOptions())
        {
            _contextFactory = contextFactory;
            _tenantContextResolver = tenantContextResolver;
        }

        protected override async Task<IReadOnlyDictionary<long, Entity.Cadastros.Colaborador>> LoadBatchAsync(
           IReadOnlyList<long> keys,
           CancellationToken cancellationToken)
        {
            await using var context = await _contextFactory.CreateDbContextAsync(cancellationToken);
            var activeContext = await _tenantContextResolver.ResolveAsync(context, cancellationToken);

            var query = context.Colaboradores
                .Include(m => m.Empresa)
                .Include(m => m.Setor)
                .Where(p => keys.Contains(p.Id));
            if (!activeContext.IsAdmin && activeContext.TenantId is long tenantId)
                query = query.Where(c => context.ColaboradorEmpresas.Any(link =>
                    link.ColaboradorId == c.Id && link.Ativo && link.Empresa.TenantId == tenantId
                    && (activeContext.IsAdmin || context.UsuariosEmpresas.Any(m => m.UsuarioId == activeContext.UserId
                        && m.EmpresaId == link.EmpresaId && m.Ativo))));
            if (!activeContext.IsAdmin && activeContext.CompanyId is long companyId)
                query = query.Where(c => context.ColaboradorEmpresas.Any(link =>
                    link.ColaboradorId == c.Id && link.EmpresaId == companyId && link.Ativo));

            var result = await query.ToDictionaryAsync(p => p.Id, cancellationToken);

            return result;
        }
    }
}
