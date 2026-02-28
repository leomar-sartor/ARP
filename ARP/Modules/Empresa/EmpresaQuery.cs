using ARP.Modules.Empresa.Types;
using HotChocolate.Language;
using HotChocolate.Resolvers;

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

        [GraphQLDescription("Buscar Empresas com Paginação, Filtragem e Ordenação")]
        [UsePaging(IncludeTotalCount = true)] 
        [UseFiltering] 
        [UseSorting]   
        public IEnumerable<Entity.Empresa> GetEmpresas(
            IResolverContext context,
            [GraphQLType(typeof(EmpresaFilterInput))] IValueNode? filter,
            [GraphQLType(typeof(EmpresaSortInput))] IValueNode? sort,
            int? skip = 0,
            int? take = 10)
        {
            return new List<Entity.Empresa>
            {
                new Entity.Empresa
                {
                    Id = 1,
                    RazaoSocial = "Teste",
                    Descricao = "Empresa teste"
                }
            };
        }
    }
}
