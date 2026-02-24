using ARP.Service.Modules.Auth;
using HotChocolate.Authorization;

namespace ARP.Modules.Auth
{

    [ExtendObjectType("Mutation")]
    public class AuthMutation
    {
        private readonly AuthService _authService;
        private readonly ILogger<AuthMutation> _logger;

        public AuthMutation(
            ILogger<AuthMutation> logger,
            AuthService authService)
        {
            _logger = logger;
            _authService = authService;
        }

        [GraphQLDescription("Registra um novo usuário.")]
        public async Task<AuthType> RegisterAsync(string username, string password)
        {
            _logger.Log(LogLevel.Information, "Registrando");

            return await _authService.RegisterAsync(username, password);
        }
    }
}
