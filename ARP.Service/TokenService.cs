using ARP.Entity.Cadastros;
using ARP.Service.Modules.Auth;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace ARP.Service
{
    public static class TokenService
    {
        private const int DefaultJwtExpirationHours = 8;

        /// <summary>
        /// Generates an auth payload with a JWT signed by <c>JWT_KEY</c>.
        /// </summary>
        /// <param name="user">Authenticated user.</param>
        /// <param name="config">App configuration (User Secrets / env / appsettings).</param>
        /// <returns>Access token and basic user info.</returns>
        public static AuthType GenerateToken(Usuario user, IConfiguration config)
        {
            var claimsIdentity = new ClaimsIdentity();

            var secret = ResolveJwtKey(config);

            if (string.IsNullOrWhiteSpace(secret))
                throw new InvalidOperationException(
                    "JWT_KEY is not set. Configure env var JWT_KEY, User Secrets, or ConnectionStrings:JWT_KEY.");

            var key = Encoding.ASCII.GetBytes(s: secret);
            var expirationHours = ResolveJwtExpirationHours(config);

            var tokenConfig = new SecurityTokenDescriptor
            {
                Subject = claimsIdentity,
                Expires = DateTime.UtcNow.AddHours(expirationHours),
                SigningCredentials = new SigningCredentials(
                    new SymmetricSecurityKey(key),
                        SecurityAlgorithms.HmacSha256Signature
                    )
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            var token = tokenHandler.CreateToken(tokenConfig);
            var tokenString = tokenHandler.WriteToken(token);

            var result = new AuthType()
            {
                Token = tokenString,
                User = new UserType()
                {
                    Id = user.Id,
                    Username = user.UserName ?? "",
                    Email = user.Email ?? ""
                },
            };

            return result;
        }

        /// <summary>
        /// Generates a JWT for the given user using configured expiration.
        /// </summary>
        /// <param name="user">Authenticated user.</param>
        /// <param name="config">App configuration (User Secrets / env / appsettings).</param>
        /// <returns>Signed JWT string.</returns>
        public static string GenerateJwt(Usuario user, IConfiguration config)
        {
            var jwtKey = ResolveJwtKey(config);
            if (string.IsNullOrWhiteSpace(jwtKey))
                throw new InvalidOperationException(
                    "JWT_KEY is not set. Configure env var JWT_KEY, User Secrets, or ConnectionStrings:JWT_KEY.");

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));

            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Email, user.Email ?? "")
            };

            var expirationHours = ResolveJwtExpirationHours(config);

            var token = new JwtSecurityToken(
                claims: claims,
                expires: DateTime.UtcNow.AddHours(expirationHours),
                signingCredentials: credentials
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        /// <summary>
        /// Resolves JWT signing key from environment, then configuration.
        /// </summary>
        private static string? ResolveJwtKey(IConfiguration? configuration)
        {
            var fromEnv = Environment.GetEnvironmentVariable("JWT_KEY");
            if (!string.IsNullOrWhiteSpace(fromEnv))
                return fromEnv;

            if (configuration is null)
                return null;

            return configuration.GetConnectionString("JWT_KEY")
                ?? configuration["JWT_KEY"];
        }

        /// <summary>
        /// Resolves JWT lifetime in hours from environment, then User Secrets / configuration.
        /// </summary>
        private static int ResolveJwtExpirationHours(IConfiguration configuration)
        {
            var raw =
                Environment.GetEnvironmentVariable("JWT_EXPIRATION_HOURS")
                ?? configuration["JWT_EXPIRATION_HOURS"];

            if (!string.IsNullOrWhiteSpace(raw)
                && int.TryParse(raw, out var hours)
                && hours > 0)
            {
                return hours;
            }

            return DefaultJwtExpirationHours;
        }
    }
}
