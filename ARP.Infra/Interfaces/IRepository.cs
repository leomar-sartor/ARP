using Dapper;
using System.Data;

namespace ARP.Infra.Interfaces
{
    public interface IRepository<T>
    {
        Task<long> InserirAsync(T dominio, IDbTransaction? transaction = null);
        Task<long> AlterarAsync(T dominio, IDbTransaction? transaction = null);
        Task<long> SalvarAsync(T dominio, IDbTransaction? transaction = null);
        Task<T> BuscarAsync(long Id, IDbTransaction? transaction = null);
        Task<T> BuscarAsync(T dominio, IDbTransaction? transaction = null);
        Task<IEnumerable<T>> BuscarTodosAsync(IDbTransaction? transaction = null);
        IEnumerable<T> BuscarTodosFilter(SqlBuilder sqlBuilder, int paginaAtual, out int totalLinhas, int? perPage = 10);
        Task ExcluirAsync(long Id, IDbTransaction? transaction = null);
        Task ExcluirAsync(T dominio, IDbTransaction? transaction = null);
    }
}
