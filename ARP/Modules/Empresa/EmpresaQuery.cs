using ARP.Infra;
using ARP.Entity.Cadastros;
using ARP.Modules.Auth;
using ARP.Modules.Empresa.Loaders;
using Microsoft.EntityFrameworkCore;

namespace ARP.Modules.Empresa
{
    [ExtendObjectType("Query")]
    public class EmpresaQuery
    {
        private readonly ILogger<EmpresaQuery> _logger;

        public EmpresaQuery(
            ILogger<EmpresaQuery> logger
            )
        {
            _logger = logger;
        }

        [GraphQLDescription("Buscar Empresas")]
        [UsePaging(IncludeTotalCount = true)]
        [UseProjection]
        [UseFiltering] 
        [UseSorting]   
        public async Task<IQueryable<Entity.Cadastros.Empresa>> GetEmpresas(
            [Service] Context context,
            [Service] TenantContextResolver tenantContextResolver,
            [Service] PermissionAuthorizationService permissionAuthorizationService,
            CancellationToken cancellationToken
            )
        {
            _logger.Log(LogLevel.Information, "Buscando Empresa(s)");

            var activeContext = await tenantContextResolver.ResolveAsync(context, cancellationToken);
            await permissionAuthorizationService.RequireAsync(
                context, activeContext, PermissionCodes.CompanyView, cancellationToken);

            var empresas = context.Empresas.AsQueryable();
            if (!activeContext.IsAdmin && activeContext.TenantId is long tenantId)
                empresas = empresas.Where(e => e.TenantId == tenantId);
            if (!activeContext.IsAdmin && activeContext.CompanyId is long companyId)
                empresas = empresas.Where(e => e.Id == companyId);
            if (!activeContext.IsAdmin)
                empresas = empresas.Where(e => context.UsuariosEmpresas.Any(m =>
                    m.UsuarioId == activeContext.UserId && m.EmpresaId == e.Id && m.Ativo));

            return empresas;
        }

        [GraphQLDescription("Buscar por empresa")]
        public async Task<Entity.Cadastros.Empresa?> GetEmpresaById(
        long id,
        EmpresaByIdDataLoader dataLoader,
        CancellationToken cancellationToken)
        {
            return await dataLoader.LoadAsync(id, cancellationToken);
        }
    }
}
