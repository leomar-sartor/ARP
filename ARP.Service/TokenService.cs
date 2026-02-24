using ARP.Entity;
using ARP.Service.Modules.Auth;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace ARP.Service
{
    public class TokenService
    {
        public static AuthType GenerateToken(Usuario user)
        {
            var claimsIdentity = new ClaimsIdentity();

            var secret = Environment.GetEnvironmentVariable("JWT_KEY");
            var key = Encoding.ASCII.GetBytes(secret);

            var tokenConfig = new SecurityTokenDescriptor
            {
                Subject = claimsIdentity,
                Expires = DateTime.UtcNow.AddHours(12),
                SigningCredentials = new SigningCredentials(
                    new SymmetricSecurityKey(key),
                        SecurityAlgorithms.HmacSha256Signature
                    )
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            var token = tokenHandler.CreateToken(tokenConfig);
            var tokenString = tokenHandler.WriteToken(token);

            var result =  new AuthType()
            {
                Token = tokenString,
                User = new UserType()
                {
                    Id = user.Id,
                    Username = user.UserName,
                    Email = user.Email
                },
            };

            return result;
        }
    }
}
