namespace ARP.Entity
{
    public class RefreshToken
    {
        public long Id { get; set; }

        public string Token { get; set; } = default!;

        public DateTime Expiration { get; set; }

        public bool Revoked { get; set; }

        public DateTime? RevokedAt { get; set; }

        public long UserId { get; set; }
        public Usuario User { get; set; } = default!;
    }
}
