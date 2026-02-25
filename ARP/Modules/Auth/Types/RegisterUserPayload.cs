namespace ARP.Modules.Auth.Types
{
    public record RegisterUserPayload(
        long? UserId,
        bool Success,
        string? Error
    );
}
