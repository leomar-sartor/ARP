using ARP.Entity.Cadastros;

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

    //public class RefreshToken2
    //{
    //    public Guid Id { get; init; } = Guid.NewGuid();

    //    // FK para o usuário dono do token
    //    public Guid UserId { get; init; }
    //    public Usuario User { get; init; } = null!;

    //    // O valor que vai no cookie httpOnly
    //    public string Token { get; init; } = string.Empty;

    //    public DateTime ExpiresAt { get; init; }
    //    public DateTime CreatedAt { get; init; } = DateTime.UtcNow;

    //    // Quando foi revogado (null = ainda válido)
    //    public DateTime? RevokedAt { get; private set; }

    //    // Qual token o substituiu após rotation
    //    public string? ReplacedByToken { get; private set; }

    //    public bool IsExpired => DateTime.UtcNow >= ExpiresAt;
    //    public bool IsRevoked => RevokedAt is not null;
    //    public bool IsActive => !IsExpired && !IsRevoked;

    //    public void Revoke(string replacedByToken)
    //    {
    //        RevokedAt = DateTime.UtcNow;
    //        ReplacedByToken = replacedByToken;
    //    }
    //}
}
