namespace ARP.Modules.Auth.Types
{
    public record UserPayload(
        long Id,
        string? Email,
        string? UserName,
        string[]? Roles,
        long EmpresaId
    );
}
