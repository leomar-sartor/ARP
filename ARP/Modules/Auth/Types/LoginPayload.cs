namespace ARP.Modules.Auth.Types
{
    public record LoginPayload(
        bool Success,
        string? Message,
        string? AccessToken,
        string? RefreshToken
    );
}
