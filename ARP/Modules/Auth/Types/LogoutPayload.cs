namespace ARP.Modules.Auth.Types
{
    public record LogoutPayload(
        bool Success,
        string? Message
    );
}
