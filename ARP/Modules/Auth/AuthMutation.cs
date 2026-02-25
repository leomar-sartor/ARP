using ARP.Entity;
using ARP.Infra;
using ARP.Modules.Auth.Types;
using ARP.Service;
using ARP.Service.Modules.Auth;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;

namespace ARP.Modules.Auth
{

    [ExtendObjectType("Mutation")]
    public class AuthMutation
    {
        private readonly ILogger<AuthMutation> _logger;

        public AuthMutation(
            ILogger<AuthMutation> logger,
            AuthService authService)
        {
            _logger = logger;
        }

        public async Task<LoginPayload> Login(
        LoginInput input,
        [Service] UserManager<Usuario> userManager,
        [Service] Context context,
        [Service] IConfiguration config)
        {
            var user = await userManager.FindByEmailAsync(input.Email);

            if (user == null)
                return new LoginPayload(false, "Usuário não encontrado", null, null);

            var validPassword = await userManager.CheckPasswordAsync(user, input.Password);

            if (!validPassword)
                return new LoginPayload(false, "Senha inválida", null, null);

            var accessToken = TokenService.GenerateJwt(user, config);

            var refreshToken = new RefreshToken
            {
                Token = Guid.NewGuid().ToString(),
                Expiration = DateTime.UtcNow.AddDays(7),
                UserId = user.Id
            };

            context.RefreshTokens.Add(refreshToken);
            await context.SaveChangesAsync();

            return new LoginPayload(
                true,
                "Login realizado com sucesso",
                accessToken,
                refreshToken.Token
            );
        }

        public async Task<RefreshTokenPayload> RefreshToken(
            RefreshTokenInput input,
            [Service] Context context,
            [Service] UserManager<Usuario> userManager,
            [Service] IConfiguration config)
        {
            var storedToken = await context.RefreshTokens
                .Include(x => x.User)
                .FirstOrDefaultAsync(x => x.Token == input.RefreshToken);

            if (storedToken == null)
                return new RefreshTokenPayload(false, "Refresh token inválido", null, null);

            if (storedToken.Revoked)
                return new RefreshTokenPayload(false, "Refresh token já revogado", null, null);

            if (storedToken.Expiration < DateTime.UtcNow)
                return new RefreshTokenPayload(false, "Refresh token expirado", null, null);

            // Revoga o token antigo
            storedToken.Revoked = true;
            storedToken.RevokedAt = DateTime.UtcNow;

            var user = storedToken.User;

            var newAccessToken = TokenService.GenerateJwt(user, config);

            var newRefreshToken = new RefreshToken
            {
                Token = Guid.NewGuid().ToString(),
                Expiration = DateTime.UtcNow.AddDays(7),
                UserId = user.Id
            };

            context.RefreshTokens.Add(newRefreshToken);

            await context.SaveChangesAsync();

            return new RefreshTokenPayload(
                true,
                "Token renovado com sucesso",
                newAccessToken,
                newRefreshToken.Token
            );
        }

        [Authorize]
        public async Task<LogoutPayload> Logout(
            ClaimsPrincipal claims,
            [Service] Context context)
        {
            var userIdClaim = claims.FindFirst(ClaimTypes.NameIdentifier);

            if (userIdClaim == null)
                return new LogoutPayload(false, "Usuário não identificado");

            var userId = long.Parse(userIdClaim.Value);

            var tokens = await context.RefreshTokens
                .Where(x => x.UserId == userId && !x.Revoked)
                .ToListAsync();

            foreach (var token in tokens)
            {
                token.Revoked = true;
                token.RevokedAt = DateTime.UtcNow;
            }

            await context.SaveChangesAsync();

            return new LogoutPayload(true, "Logout realizado com sucesso");
        }

        public async Task<RegisterUserPayload> RegisterUserAsync(
            RegisterUserInput input,
            [Service] UserManager<Usuario> userManager
        )
        {
            _logger.Log(LogLevel.Information, "Registrando");

            var user = new Usuario
            {
                Cpf = input.Cpf,
                UserName = input.UserName,
                Email = input.Email,
                EmpresaId = input.EmpresaId
            };

            try
            {
                var result = await userManager.CreateAsync(user, input.Password);

                if (!result.Succeeded)
                {
                    var errors = string.Join(", ", result.Errors.Select(e => e.Description));

                    return new RegisterUserPayload(
                        null,
                        false,
                        errors
                    );
                }

                return new RegisterUserPayload(
                    user.Id,
                    true,
                    "Usuário criado com sucesso"
                );
            }
            catch (Exception e)
            {
                return new RegisterUserPayload(
                        null,
                        false,
                        e.Message
                    );
            }
        }
    }
}
