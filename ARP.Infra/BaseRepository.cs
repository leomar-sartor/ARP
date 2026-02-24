using ARP.Infra.Interfaces;
using Dapper;
using System.Data;

namespace ARP.Infra
{
    public abstract class BaseRepository<T> : Repository where T : class
    {
        protected string _table;
        protected string _tableAlias;
        protected string _select;
        public BaseRepository(
            IConexao conexao,
            string table,
            string tableAlias,
            string select
            ) : base(conexao)
        {
            _table = table;
            _tableAlias = tableAlias;
            _select = string.Format(select, table, tableAlias);
        }

        protected abstract object GetParameters(T entity);

        public async Task<long> InserirAsync(T entity, IDbTransaction? transaction = null, bool? withId = false)
            => await InsertAsync(_table, GetParameters(entity), transaction, withId);

        public async Task<long> AlterarAsync(T entity, IDbTransaction? transaction = null)
        {
            await UpdateAsync(_table, GetParameters(entity), transaction);
            return (long)typeof(T).GetProperty("Id")?.GetValue(entity)!;
        }

        public async Task<long> SalvarAsync(T entity, IDbTransaction? transaction = null, bool? withId = false)
        {
            var obj = await BuscarAsync(entity, transaction);
            if (obj == null)
                return await InserirAsync(entity, transaction, withId);
            else
                return await AlterarAsync(entity, transaction);
        }

        public async Task<IEnumerable<T>> BuscarTodosAsync(IDbTransaction? transaction = null)
            => await _conexao.QueryAsync<T>($"{_select} Where {_tableAlias}.DeletedAt IS NULL", null, transaction);

        public IEnumerable<T> BuscarTodosFilter(SqlBuilder sqlBuilder, int paginaAtual, out int totalLinhas, int? perPage = 10)
        {
            var sql = sqlBuilder.AddTemplate($"{_select} /**innerjoin**/ /**leftjoin**/ /**where**/ /**orderby**/");
            return _conexao.Query<T>(sql.RawSql, paginaAtual, out totalLinhas, sql.Parameters, perPage);
        }

        public async Task<IEnumerable<T>> BuscarTodosFilter(SqlBuilder sqlBuilder)
        {
            var sql = sqlBuilder.AddTemplate($"{_select} /**innerjoin**/ /**where**/ /**orderby**/");
            return await _conexao.QueryAsync<T>(sql.RawSql, sql.Parameters);
        }

        public async Task<T?> BuscarAsync(SqlBuilder sqlBuilder, long id, IDbTransaction? transaction = null)
        {
            sqlBuilder.Where($"{_tableAlias}.Id = @Id", new { Id = id });
            sqlBuilder.Where($"{_tableAlias}.DeletedAt IS NULL");

            var sql = sqlBuilder.AddTemplate($"{_select} /**innerjoin**/ /**where**/ /**orderby**/");
            return await _conexao.QueryFirstOrDefaultAsync<T>(sql.RawSql, sql.Parameters, transaction);
        }

        public async Task<T?> BuscarAsync(long id, IDbTransaction? transaction = null)
        {
            var sql = $"{_select} Where {_tableAlias}.Id = @Id AND {_tableAlias}.DeletedAt IS NULL";
            return await _conexao.QueryFirstOrDefaultAsync<T>(sql, new { Id = id }, transaction);
        }

        public async Task<T?> BuscarAsync(T entity, IDbTransaction? transaction = null)
        {
            var id = (long)typeof(T).GetProperty("Id")?.GetValue(entity)!;
            return await BuscarAsync(id, transaction);
        }

        public async Task ExcluirAsync(long id, IDbTransaction? transaction = null)
            => await DeleteAsync(_table, new { Id = id });

        public async Task ExcluirAsync(T entity, IDbTransaction? transaction = null)
        {
            var id = (long)typeof(T).GetProperty("Id")?.GetValue(entity)!;
            await ExcluirAsync(id, transaction);
        }
    }
}
