using ARP.Service.Modules.Auth;
using HotChocolate.Authorization;

namespace ARP.Modules.Auth
{
    [ExtendObjectType("Query")]
    public class AuthQuery
    {
        private readonly ILogger<AuthQuery> _logger;

        public AuthQuery(
            ILogger<AuthQuery> logger, 
            AuthService authService)
        {
            _logger = logger;
        }

        
        [GraphQLDescription("Faz login de um usuário existente.")]
        public async Task<string> LoginAsync(AuthInput input)
        {
            _logger.Log(LogLevel.Information, "Realizando Login");
            _logger.Log(LogLevel.Warning, "Realizando Login");
            _logger.Log(LogLevel.Error, "Realizando Login");
            _logger.Log(LogLevel.Critical, "Realizando Login");

            return "login";
        }

        [Authorize]
        [GraphQLDescription("Teste de Login com JWT.")]
        public async Task<string> VerificarLoginAsync(AuthInput input)
        {
            return "teste";
        }
    }
}
