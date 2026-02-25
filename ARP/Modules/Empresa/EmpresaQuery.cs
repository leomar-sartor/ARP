using ARP.Modules.Empresa.Types;
using ARP.Service.Modules.Empresa;
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

        }

        [GraphQLDescription("Buscar Empresas com Paginação, Filtragem e Ordenação")]
        [UsePaging(IncludeTotalCount = true)] // UsePaging is available in HotChocolate 15.x
        [UseFiltering] // Habilita filtragem
        [UseSorting]   // Habilita ordenação
        public IEnumerable<Entity.Empresa> GetEmpresas(
            IResolverContext context,
            [GraphQLType(typeof(EmpresaFilterInput))] IValueNode? filter,
            [GraphQLType(typeof(EmpresaSortInput))] IValueNode? sort,
            int? skip = 0,
            int? take = 10)
        {
            var empresa = new ARP.Entity.Empresa
            {
                RazaoSocial = "",
                Descricao = ""
            };

            var lista = new List<Entity.Empresa>() { 
                empresa
            };
                
            return lista;
        }
    }
}
