namespace ARP.Modules.Auth.Types
{
    [GraphQLDescription("Dados de entrada para operações de autenticação.")]
    public record LoginInput(

        [GraphQLDescription("Identificador do usuário (e-mail ou nome de usuário).")]
        string Email,

        [GraphQLDescription("Senha do usuário.")]
        string Password
    );
}
