using ARP.Entity;
using ARP.Entity.Cadastros;
using ARP.Infra;
using ARP.Modules.Auth.Types;
using ARP.Service;
using ARP.Service.Modules.Auth;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;

namespace ARP.Modules.Auth
{
    [ExtendObjectType("Mutation")]
    public class AuthMutation
    {
        private readonly ILogger<AuthMutation> _logger;
        private readonly IWebHostEnvironment _env;


        public AuthMutation(
            IWebHostEnvironment env,
            ILogger<AuthMutation> logger,
            AuthService authService)
        {
            _env = env;
            _logger = logger;
        }

        [AllowAnonymous]
        public async Task<LoginPayload> Login(
        [Service] IHttpContextAccessor httpContextAccessor,
        LoginInput input,
        [Service] Context context,
        [Service] UserManager<Usuario> userManager,
        [Service] IConfiguration config)
        {
            try
            {
                var user = await userManager.FindByEmailAsync(input.Email);

                if (user == null)
                    return new LoginPayload(false, "Usuário não encontrado", null, null);

                var validPassword = await userManager.CheckPasswordAsync(user, input.Password);

                if (!validPassword)
                    return new LoginPayload(false, "Senha inválida", null, null);

                var accessToken = TokenService.GenerateJwt(user, config);

                var bytes = RandomNumberGenerator.GetBytes(64);
                var nRefreshToken = Convert.ToBase64String(bytes)
                    .Replace("+", "-")
                    .Replace("/", "_")
                    .Replace("=", "");

                var refreshToken = new RefreshToken
                {
                    Token = nRefreshToken,
                    Expiration = DateTime.UtcNow.AddDays(7),
                    UserId = user.Id
                };

                context.RefreshTokens.Add(refreshToken);
                await context.SaveChangesAsync();

                var isDevelopment = _env.IsDevelopment();
                CookieOptions optionsCookie;
                if (isDevelopment)
                {
                    optionsCookie = new CookieOptions
                    {
                        HttpOnly = true,
                        Secure = !isDevelopment,                                    // false em dev
                        SameSite = isDevelopment ? SameSiteMode.Lax : SameSiteMode.None, // Lax em dev
                        Expires = DateTimeOffset.UtcNow.AddDays(7)
                    };
                }
                else
                {
                    optionsCookie = new CookieOptions
                    {
                        HttpOnly = true,       // JS nunca acessa
                        Secure = true,         // Apenas HTTPS
                        SameSite = SameSiteMode.Strict,
                        Expires = DateTimeOffset.UtcNow.AddDays(7)
                    };
                }

                // Seta o refresh token como httpOnly cookie
                httpContextAccessor.HttpContext!.Response.Cookies.Append(
                    "refresh_token",
                    refreshToken.Token,
                    optionsCookie
                );

                return new LoginPayload(
                    Success: true,
                    Message: "Login realizado com sucesso",
                    AccessToken: accessToken,
                    User: new UserPayload
                    (
                        Id: user.Id,
                        Email: user.Email,
                        UserName: user.UserName,
                        Roles: new string[] { "Admin", "User" },
                        EmpresaId: 0
                        //(await userManager.GetRolesAsync(user)).ToArray()
                    )
                );
            }
            catch (Exception e)
            {
                return new LoginPayload(
                    Success: false,
                    Message: e.Message,
                    AccessToken: null,
                    User: new UserPayload
                        (
                            Id: 0,
                            Email: null,
                            UserName: null,
                            Roles: Array.Empty<string>(),
                            EmpresaId: 0
                        )
                );
            }
        }

        [AllowAnonymous]
        public async Task<RefreshTokenPayload> RefreshToken(
            [Service] IHttpContextAccessor httpContextAccessor,
            [Service] Context context,
            [Service] UserManager<Usuario> userManager,
            [Service] IConfiguration config)
        {
            // Lê o cookie que o browser envia automaticamente
            var refreshToken = httpContextAccessor.HttpContext!
                .Request.Cookies["refresh_token"];

            if (string.IsNullOrEmpty(refreshToken))
                throw new GraphQLException("Sessão não encontrada.");

            var storedToken = await context.RefreshTokens
                .Include(x => x.User)
                .FirstOrDefaultAsync(x => x.Token == refreshToken);

            if (storedToken == null)
                return new RefreshTokenPayload(false, "Refresh token inválido", null, null);

            if (storedToken.Revoked)
                return new RefreshTokenPayload(false, "Refresh token já revogado", null, null);

            if (storedToken.Expiration < DateTime.UtcNow)
                return new RefreshTokenPayload(false, "Refresh token expirado", null, null);

            var user = storedToken.User;
            var newAccessToken = TokenService.GenerateJwt(user, config);

            #region SE QUISER MAIS SEGURANÇA
            // Revoga o token antigo -AKI
            //storedToken.Revoked = true;
            //storedToken.RevokedAt = DateTime.UtcNow;
            //var bytes = RandomNumberGenerator.GetBytes(64);
            //var nRefreshToken = Convert.ToBase64String(bytes)
            //    .Replace("+", "-")
            //    .Replace("/", "_")
            //    .Replace("=", "");
            //var newRefreshToken = new RefreshToken
            //{
            //    Token = nRefreshToken,
            //    Expiration = DateTime.UtcNow.AddDays(7),
            //    UserId = user.Id
            //};
            //context.RefreshTokens.Add(newRefreshToken);

            //await context.SaveChangesAsync();

            //var isDevelopment = _env.IsDevelopment();
            //CookieOptions optionsCookie;
            //if (isDevelopment)
            //{
            //    optionsCookie = new CookieOptions
            //    {
            //        HttpOnly = true,
            //        Secure = !isDevelopment,                                    // false em dev
            //        SameSite = isDevelopment ? SameSiteMode.Lax : SameSiteMode.None, // Lax em dev
            //        Expires = DateTimeOffset.UtcNow.AddDays(7)
            //    };
            //}
            //else
            //{
            //    optionsCookie = new CookieOptions
            //    {
            //        HttpOnly = true,       // JS nunca acessa
            //        Secure = true,         // Apenas HTTPS
            //        SameSite = SameSiteMode.Strict,
            //        Expires = DateTimeOffset.UtcNow.AddDays(7)
            //    };
            //}

            //// Rotation: seta o NOVO refresh token no cookie, substituindo o antigo
            //httpContextAccessor.HttpContext.Response.Cookies.Append(
            //    "refresh_token",
            //    newRefreshToken.Token,
            //    optionsCookie);
            #endregion

            return new RefreshTokenPayload(
                Success: true,
                Message: "Token renovado com sucesso",
                AccessToken: newAccessToken,
                User: new UserPayload
                    (
                        Id: user.Id,
                        Email: user.Email,
                        UserName: user.UserName,
                        Roles: new string[] { "Admin", "User" },
                        EmpresaId: 0
                        //(await userManager.GetRolesAsync(user)).ToArray()
                    )
            );
        }

        [AllowAnonymous]
        public async Task<LogoutPayload> Logout(
            [Service] IHttpContextAccessor httpContextAccessor,
            [Service] Context context)
        {
            var refreshToken = httpContextAccessor.HttpContext!
                .Request.Cookies["refresh_token"];

            var storedToken = await context.RefreshTokens
                .SingleOrDefaultAsync(rt => rt.Token == refreshToken);

            // Token não existe ou já foi revogado — não faz nada silenciosamente
            // Evita expor se um token existe ou não (segurança por obscuridade)
            if (storedToken is null || storedToken.Revoked)
                return new LogoutPayload(false, "");

            storedToken.Revoked = true;
            storedToken.RevokedAt = DateTime.UtcNow;

            await context.SaveChangesAsync();

            httpContextAccessor.HttpContext.Response.Cookies.Delete("refresh_token");

            return new LogoutPayload(true, "Logout realizado com sucesso");
        }

        [Authorize]
        public async Task<RegisterUserPayload> RegisterUserAsync(
            RegisterUserInput input,
            [Service] UserManager<Usuario> userManager,
            [Service] Context context
        )
        {
            _logger.Log(LogLevel.Information, "Registrando");

            var cpf = Utils.CpfHelper.OnlyDigits(input.Cpf);

            if (!Utils.CpfHelper.IsValidCpf(cpf))
            {
                return new RegisterUserPayload(
                    null,
                    false,
                    "CPF inválido"
                );
            }

            var cpfAlreadyExists = await context.Users
                .AnyAsync(u => u.Cpf == cpf);

            if (cpfAlreadyExists)
            {
                return new RegisterUserPayload(
                    null,
                    false,
                    "Já existe um cadastro com este CPF."
                );
            }

            var user = new Usuario
            {
                Cpf = cpf,
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
            catch (DbUpdateException)
            {
                return new RegisterUserPayload(
                        null,
                        false,
                        "Já existe um cadastro com este CPF."
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