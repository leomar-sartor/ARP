using ARP.Service.Modules.Auth;

namespace ARP.Modules.Auth
{

    [ExtendObjectType("Mutation")]
    public class AuthMutation
    {
        private readonly ILogger<AuthMutation> _logger;

        public AuthMutation(
            ILogger<AuthMutation> logger,
            AuthService authService)
        {
            _logger = logger;
        }

        [GraphQLDescription("Registra um novo usuário.")]
        public async Task<string> RegisterAsync(string username, string password)
        {
            _logger.Log(LogLevel.Information, "Registrando");

            return "teste";
        }
    }
}
