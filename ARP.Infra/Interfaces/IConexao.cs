using System.Data;

namespace ARP.Infra.Interfaces
{
    public interface IConexao
    {
        Task<IDbTransaction> BeginTransactionAsync();
        Task<IDbTransaction> BeginTransactionAsyncWithLevel(IsolationLevel? level = null);
        Task<T> QueryFirstAsync<T>(string sql, object? param = null, IDbTransaction? transacao = null);
        Task<T> QueryFirstOrDefaultAsync<T>(string sql, object? param = null, IDbTransaction? transacao = null);
        Task<IEnumerable<T>> QueryAsync<T>(string sql, object? param = null, IDbTransaction? transacao = null);
        IEnumerable<T> Query<T>(string sql, int pagina, out int totalLinhas, object? param = null, int? porPagina = 10);
        Task<long> ExecuteAsync<T>(string sql, string tabela, object? param = null, IDbTransaction? transacao = null);
    }
}
