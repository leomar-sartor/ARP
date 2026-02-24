using ARP.Infra.Interfaces;
using ARP.Modules.Empresa.Types;
using ARP.Service.Modules.Empresa;
using HotChocolate.Data;
using HotChocolate.Data.Filters;
using HotChocolate.Data.Sorting;
using HotChocolate.Language;
using HotChocolate.Resolvers;
using HotChocolate.Types.Pagination;
using System.Text;

namespace ARP.Modules.Empresa
{
    [ExtendObjectType("Query")]
    public class EmpresaQuery
    {
        private readonly IConexao _conexao;
        private readonly ILogger<EmpresaQuery> _logger;

        private readonly EmpresaService _empresaService;

        public EmpresaQuery(
            IConexao conexao,
            ILogger<EmpresaQuery> logger
            )
        {
            _conexao = conexao;
            _logger = logger;

            _empresaService = new EmpresaService(_conexao);
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
            var first = context.ArgumentValue<int?>("first");
            var after = context.ArgumentValue<string?>("after");
            var last = context.ArgumentValue<int?>("last");
            var before = context.ArgumentValue<string?>("before");

            _logger.Log(LogLevel.Information, "Buscando Empresas Paginadas");

            var res = _empresaService.GetEmpresasPaginadas(skip, take, filter, sort);

            return res;
        }
    }
}
