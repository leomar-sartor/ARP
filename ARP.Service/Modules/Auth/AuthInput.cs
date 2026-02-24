namespace ARP.Service.Modules.Auth
{
    [GraphQLDescription("Dados de entrada para operações de autenticação.")]
    public class AuthInput
    {
        [GraphQLDescription("Identificador do usuário (e-mail ou nome de usuário).")]
        public string Username { get; set; } = default!;

        [GraphQLDescription("Senha do usuário.")]
        public string Password { get; set; } = default!;

    }
}
