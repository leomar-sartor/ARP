using ARP.Infra.Interfaces;
using System.Data;

namespace ARP.Infra
{
    public class SqlUtil
    {
        public IConexao _conexao;

        public SqlUtil(IConexao conexao) => _conexao = conexao;

        public async Task<long> InsertAsync(string nomeTabela, object colunas, IDbTransaction? transacao = null, bool? withId = false)
        {
            var setColunas = new List<string>();
            var setValores = new List<string>();

            IEnumerable<System.Reflection.PropertyInfo>? objColunas = null;
            if (withId == false)
                objColunas = colunas.GetType().GetProperties().Where(c => c.Name != "Id");
            else
                objColunas = colunas.GetType().GetProperties();

            foreach (var coluna in objColunas)
            {
                setColunas.Add(coluna.Name);
                setValores.Add($"@{coluna.Name}");
            }

            var sql = $"INSERT INTO {nomeTabela} ({string.Join(",", setColunas)}) VALUES ({string.Join(",", setValores)})";

            return await _conexao.ExecuteAsync<long>(sql, nomeTabela, colunas, transacao);
        }

        public async Task<long> UpdateAsync(string nomeTabela, object colunas, IDbTransaction? transacao = null, string chave = "Id")
        {
            var setColunas = new List<string>();
            var objColunas = colunas.GetType().GetProperties().Where(c => c.Name != "CreatedAt");

            foreach (var coluna in objColunas)
            {
                var nome = coluna.Name;
                if (nome == chave)
                    continue;

                setColunas.Add($"{nome} = @{nome}");
            }

            var sql = $"UPDATE {nomeTabela} SET {string.Join(",", setColunas)} WHERE {chave} = @{chave}";

            return await _conexao.ExecuteAsync<long>(sql, nomeTabela, colunas, transacao);
        }

        public async Task<long> DeleteAsync(string nomeTabela, object? colunas, IDbTransaction? transacao = null, string chave = "Id")
        {
            var sql = $"DELETE FROM {nomeTabela} WHERE {chave} = @{chave}";

            var result = await _conexao.ExecuteAsync<long>(sql, nomeTabela, colunas, transacao);

            return result;
        }
    }
}
