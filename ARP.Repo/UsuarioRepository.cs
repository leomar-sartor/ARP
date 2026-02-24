using ARP.Entity;
using ARP.Infra;
using ARP.Infra.Interfaces;
using System.Data;

namespace ARP.Repo
{
    public class UsuarioRepository : BaseRepository<Usuario>
    {
        private static readonly string select =
            @"SELECT 
                    U.Id,
                    U.Cpf,
                    U.Email,
                    U.UserName,
                    U.Password,
                    U.CreatedAt,
                    U.UpdatedAt,
                    U.DeletedAt
                 FROM 
                     {0} {1} 
                 ";

        public UsuarioRepository(IConexao conexao) : base(conexao, "Usuario", "U", select) { }

        protected override object GetParameters(Usuario dominio)
        {
            var parameters = new
            {
                dominio.Id,
                dominio.Cpf,
                dominio.Email,
                dominio.UserName,
                dominio.Password,
                dominio.CreatedAt,
                dominio.UpdatedAt
            };

            return parameters;
        }

        //Adicionais
        public async Task<Usuario> BuscarPorUsernameAndPasswordAsync(
            string username,
            string password,
            IDbTransaction? transaction = null)
        {
            var sql = $"{_select} Where {_tableAlias}.UserName = @UserName AND {_tableAlias}.DeletedAt IS NULL";
            return await _conexao.QueryFirstOrDefaultAsync<Usuario>(sql,
                new {
                    UserName = username,
                    password = password,
                },
                transaction);
        }
    }
}
