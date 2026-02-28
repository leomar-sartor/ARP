using ARP.Infra;
using ARP.Modules.Empresa.Loaders;

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
        public IEnumerable<Entity.Empresa?> GetEmpresas(
            [Service] Context context
            )
        {
            _logger.Log(LogLevel.Information, "Buscando Empresa(s)");

            return context.Empresas.AsQueryable();
        }

        [GraphQLDescription("Buscar por empresa")]
        public async Task<Entity.Empresa?> GetEmpresaById(
        long id,
        EmpresaByIdDataLoader dataLoader,
        CancellationToken cancellationToken)
        {
            return await dataLoader.LoadAsync(id, cancellationToken);
        }
    }
}
