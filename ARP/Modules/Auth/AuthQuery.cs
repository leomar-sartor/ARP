using ARP.Service.Modules.Auth;
using HotChocolate.Authorization;

namespace ARP.Modules.Auth
{
    [ExtendObjectType("Query")]
    public class AuthQuery
    {
        private readonly AuthService _authService;
        private readonly ILogger<AuthQuery> _logger;

        public AuthQuery(
            ILogger<AuthQuery> logger, 
            AuthService authService)
        {
            _logger = logger;
            _authService = authService;
        }

        
        [GraphQLDescription("Faz login de um usuário existente.")]
        public async Task<AuthType> LoginAsync(AuthInput input)
        {
            _logger.Log(LogLevel.Information, "Realizando Login");
            _logger.Log(LogLevel.Warning, "Realizando Login");
            _logger.Log(LogLevel.Error, "Realizando Login");
            _logger.Log(LogLevel.Critical, "Realizando Login");

            return await _authService.Login(input.Username, input.Password);
        }

        [Authorize]
        [GraphQLDescription("Teste de Login com JWT.")]
        public async Task<AuthType> VerificarLoginAsync(AuthInput input)
        {
            return await _authService.Login(input.Username, input.Password);
        }
    }
}
