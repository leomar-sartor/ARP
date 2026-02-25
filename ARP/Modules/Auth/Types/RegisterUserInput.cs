namespace ARP.Modules.Auth.Types
{
    public record RegisterUserInput(
        string Cpf,
        string UserName,
        string Email,
        string Password,
        long? EmpresaId
    );
}
