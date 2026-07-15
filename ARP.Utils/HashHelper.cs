using System.Security.Cryptography;
using System.Text;

namespace ARP.Utils
{
    /// <summary>
    /// HMAC-SHA256 helpers for one-way hashes keyed by an application secret.
    /// </summary>
    public static class HashHelper
    {
        /// <summary>
        /// Computes a hex-encoded HMAC-SHA256 of <paramref name="value"/> using <paramref name="secret"/>.
        /// </summary>
        /// <param name="value">Plaintext to hash (e.g. normalized CPF).</param>
        /// <param name="secret">HMAC key; must be non-empty (<c>KEY_HMAC</c>).</param>
        /// <returns>Uppercase hex digest.</returns>
        /// <exception cref="ArgumentException">Thrown when <paramref name="value"/> or <paramref name="secret"/> is null or whitespace.</exception>
        public static string Generate(string value, string secret)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(value);
            ArgumentException.ThrowIfNullOrWhiteSpace(secret);

            using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(secret));

            var hash = hmac.ComputeHash(
                Encoding.UTF8.GetBytes(value)
            );

            return Convert.ToHexString(hash);
        }
    }
}
