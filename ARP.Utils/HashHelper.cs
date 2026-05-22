using System.Security.Cryptography;
using System.Text;

namespace ARP.Utils
{
    public static class HashHelper
    {
        public static string Generate(string value, string secret)
        {
            using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(secret));

            var hash = hmac.ComputeHash(
                Encoding.UTF8.GetBytes(value)
            );

            return Convert.ToHexString(hash);
        }
    }
}
