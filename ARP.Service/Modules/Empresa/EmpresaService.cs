using ARP.Infra.Interfaces;
using ARP.Repo;
using Dapper;
using HotChocolate.Data.Filters;
using HotChocolate.Data.Sorting;
using HotChocolate.Language;
using HotChocolate.Resolvers;

namespace ARP.Service.Modules.Empresa
{
    public class EmpresaService
    {
        public IConexao _conexao;

        private readonly EmpresaRepository _rEmpresa;

        public EmpresaService(IConexao conexao)
        {
            _conexao = conexao;

            _rEmpresa = new EmpresaRepository(_conexao);
        }

        public IEnumerable<Entity.Empresa> GetEmpresasPaginadas(
            int? skip,
            int? take,
            IValueNode? filter = null,
            IValueNode? sort = null
            )
        {
            var sqlBuilder = new SqlBuilder();

            var parameters = new DynamicParameters();

            var template = sqlBuilder.AddTemplate($@"
                SELECT /**select**/ 
                FROM {_rEmpresa._table} {_rEmpresa._tableAlias}
                /**innerjoin**/
                /**leftjoin**/
                /**where**/
                /**orderby**/
                /**paging**/
            ", parameters);

            sqlBuilder.Select($"{_rEmpresa._tableAlias}.*");
            sqlBuilder.Where($"{_rEmpresa._tableAlias}.DeletedAt IS NULL");

            GraphQLSqlFilterParser.ApplyFilter<Entity.Empresa>(
            sqlBuilder,
            _rEmpresa._tableAlias,
            filter,
            parameters
            );

            GraphQLSqlSortParser.ApplySort<Entity.Empresa>(
            sqlBuilder,
            _rEmpresa._tableAlias,
            sort);

            int perPage = take ?? 10;
            int page = perPage > 0 ? ((skip ?? 0) / perPage) + 1 : 1;

            var result = _rEmpresa.BuscarTodosFilter(
                 template,
                 page,
                out int totalDeLinhas
             );

            return result;
        }

        public async Task<List<ARP.Entity.Empresa>> BuscarTodosFilter()
        {
            var results = await _rEmpresa.BuscarTodosAsync();

            return results.ToList();
        }

        public async Task<ARP.Entity.Empresa> Create(ARP.Entity.Empresa empresa)
        {
            empresa.Id = await _rEmpresa.SalvarAsync(empresa);

            return empresa;
        }

    }
}
