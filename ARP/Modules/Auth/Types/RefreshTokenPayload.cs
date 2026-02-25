namespace ARP.Modules.Auth.Types
{
    public record RefreshTokenPayload(
        bool Success,
        string? Message,
        string? AccessToken,
        string? RefreshToken
    );
}
