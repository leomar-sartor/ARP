using ARP.Entity;
using ARP.Infra.Interfaces;
using ARP.Repo;

namespace ARP.Service.Modules.Auth
{
    public class AuthService
    {
        public IConexao _conexao;

        private readonly UsuarioRepository _rUsuario;

        public AuthService(IConexao conexao)
        {
            _conexao = conexao;

            _rUsuario = new UsuarioRepository(_conexao);
        }

        public async Task<AuthType> Login(string username, string password)
        {
            var user = await _rUsuario.BuscarPorUsernameAndPasswordAsync(username, password);

            if (user == null) {
                return null;
            }

            var retorno = TokenService.GenerateToken(user);

            return retorno;
        }

        public async Task<AuthType> RegisterAsync(string username, string password)
        {
            var user = await _rUsuario.SalvarAsync(new Usuario()
            {
                UserName = username,
                Password = password,
                Email = "teste@tste.com.br",
                Cpf = "123.456.789-00"
            });

            return new AuthType()
            {
                Token = "123.456.789",
                User = new UserType()
                {
                    Id = 429,
                    Username = "leomar.sartor",
                    Email = "leomar_sartor@unochapeco.edu.br",
                }
            };
        }
    }
}
