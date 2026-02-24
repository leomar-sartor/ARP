using ARP.Infra.Interfaces;
using Dapper;
using Npgsql;
using System.Data;
using System.Text.Json;

namespace ARP.Infra
{
    public class Conexao : IConexao, IAsyncDisposable
    {
        private readonly string _stringConnection;

        public readonly NpgsqlConnection _connection;
        private ConnectionState _state;

        public Conexao()
        {
            var conn = Environment.GetEnvironmentVariable("CONNECTION_STRING");

            if (string.IsNullOrWhiteSpace(conn))
                throw new InvalidOperationException("Environment variable 'CONNECTION_STRING' is not set or is empty.");

            _stringConnection = conn;
            _connection = new NpgsqlConnection(_stringConnection);
        }

        public async Task<IDbTransaction> BeginTransactionAsync()
        {
            await CheckStateAsync();

            return await _connection.BeginTransactionAsync();
        }

        public async Task<IDbTransaction> BeginTransactionAsyncWithLevel(IsolationLevel? level = null)
        {
            await CheckStateAsync();

            if (level != null)
                return await _connection.BeginTransactionAsync((IsolationLevel)level);

            return await _connection.BeginTransactionAsync();
        }

        public T QueryFirst<T>(string sql, object? param = null, IDbTransaction? transacao = null)
        {
            try
            {
                return _connection.QueryFirst<T>(sql.ToLower(), param, transacao);
            }
            catch (Exception ex)
            {
                throw new Exception(sql, ex);
            }
        }

        public async Task<T> QueryFirstAsync<T>(string sql, object? param = null, IDbTransaction? transacao = null)
        {
            try
            {
                await CheckStateAsync();

                return await _connection.QueryFirstAsync<T>(sql.ToLower(), param, transacao);
            }
            catch (Exception ex)
            {
                throw new Exception(sql, ex);
            }
        }
        public async Task<T> QueryFirstOrDefaultAsync<T>(string sql, object? param = null, IDbTransaction? transacao = null)
        {
            try
            {
                await CheckStateAsync();

                return await _connection.QueryFirstOrDefaultAsync<T>(sql.ToLower(), param, transacao);
            }
            catch (Exception ex)
            {
                throw new Exception(sql, ex);
            }
        }

        public IEnumerable<T> Query<T>(string sql, object? param = null, IDbTransaction? transacao = null)
        {
            try
            {
                return _connection.Query<T>(sql.ToLower(), param, transacao);
            }
            catch (Exception ex)
            {
                throw new Exception(sql, ex);
            }
        }

        public async Task<IEnumerable<T>> QueryAsync<T>(string sql, object? param = null, IDbTransaction? transacao = null)
        {
            try
            {
                await CheckStateAsync();

                return await _connection.QueryAsync<T>(sql.ToLower(), param, transacao);
            }
            catch (Exception ex)
            {
                throw new Exception(sql, ex);
            }
        }

        public IEnumerable<T> Query<T>(string sql, int pagina, out int totalLinhas, object? param = null, int? porPagina = 10)
        {
            var offset = 0;
            if (pagina > 1)
                offset = ((int)porPagina * (pagina - 1));

            var totalSQL = $"select count(*) from ({sql}) as X";
            totalLinhas = QueryFirst<int>(totalSQL, param);

            if (!sql.ToLower().Contains("limit"))
                sql = $"{sql} offset {offset} rows fetch next {porPagina} rows only";

            return Query<T>(sql, param);
        }

        public async Task<long> ExecuteAsync<T>(string sql, string tabela, object? param = null, IDbTransaction? transacao = null)
        {
            try
            {
                var pontoVirgula = sql[sql.Length - 1] == ';' ? "" : ";";

                var (comand, tipo) = Mount(sql);

                var Ids = await _connection.QueryAsync<long>(comand.ToLower(), param, transacao);

                if (Ids != null && Ids.Count() == 0)
                    return 0;

                if (Ids != null && Ids.Count() == 1)
                    await GravaLog(tipo, Ids.Single(), tabela, param, null, transacao);

                if (Ids != null && Ids.Count() > 1)
                {
                    foreach (var id in Ids)
                        await GravaLog(tipo, id, tabela, param, null, transacao);

                    return Ids.FirstOrDefault();
                }

                return Ids.Single();

            }
            catch (Exception e)
            {
                transacao?.Rollback();

                await GravaLog("ERRO", 0, tabela, param, $"[{sql}]" + " : " + e.Message, transacao);

                throw new Exception(sql, e);
            }
        }

        public (string sql, string tipo) Mount(string sql)
        {
            var retornar = false;
            var tipoComando = "";

            if (sql.Contains("INSERT"))
            {
                retornar = true;
                tipoComando = "INSERT";
            }
            else if (sql.Contains("UPDATE"))
            {
                retornar = true;
                tipoComando = "UPDATE";

                if (sql.Contains("DeletedAt"))
                {
                    tipoComando = "REMOVE";
                }
            }
            else if (sql.Contains("DELETE"))
            {
                retornar = true;
                tipoComando = "DELETE";
            }

            if (retornar)
                sql = $"{sql} RETURNING Id";

            return (sql, tipoComando);
        }

        public async Task CheckStateAsync()
        {
            _state = _connection.State;

            try
            {
                if (_state == ConnectionState.Closed)
                {
                    await _connection.OpenAsync();
                    return;
                }

                if (_state == ConnectionState.Connecting)
                {
                    while (_state == ConnectionState.Connecting)
                    {
                        _state = _connection.State;

                        if (_state != ConnectionState.Connecting)
                            return;
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Erro de Conexão com o Banco de Dados!", ex);
            }
        }

        public async ValueTask DisposeAsync()
        {
            await _connection.CloseAsync();

            GC.SuppressFinalize(this);
        }

        public async Task GravaLog(string tipo, long entidadeId, string entidadeNome, object? param, string? mensagem, IDbTransaction? transacao = null)
        {
            mensagem = mensagem ?? string.Empty;
            Type obj = typeof(object);
            string json = "";

            if (param != null)
            {
                obj = param.GetType();
                json = JsonSerializer.Serialize(param);
            }

            object userId = null;

            if (obj.GetProperty("UserId") != null)
            {
                var propriedade = obj.GetProperty("UserId");
                userId = propriedade.GetValue(param, null);
            }

            var agora = DateTime.SpecifyKind(DateTime.Now, DateTimeKind.Utc);

            await _connection.ExecuteAsync(@"INSERT INTO Log (UserId, EntidadeId, Entidade, Operacao, Campos, CreatedAt, Mensagem) 
                                                VALUES (@UserId, @EntidadeId, @Entidade, @Operacao, @Campos, @CreatedAt, @Mensagem)",
            new
            {
                UserId = userId ?? 0,
                EntidadeId = entidadeId,
                Entidade = entidadeNome,
                Operacao = tipo,
                Campos = json,
                CreatedAt = agora,
                Mensagem = mensagem ?? ""
            }, transacao ?? null);
        }
    }
}