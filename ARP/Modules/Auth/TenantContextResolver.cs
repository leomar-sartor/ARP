using ARP.Entity.Cadastros;
using ARP.Infra;
using Microsoft.EntityFrameworkCore;
using System.Globalization;
using System.Security.Claims;

namespace ARP.Modules.Auth
{
    public sealed record ActiveTenantContext(
        long UserId,
        long? TenantId,
        long? CompanyId,
        bool IsAdmin);

    public sealed class TenantContextResolver(
        IHttpContextAccessor httpContextAccessor,
        PermissionAuthorizationService permissionAuthorizationService)
    {
        public async Task<ActiveTenantContext> ResolveAsync(
            Context context,
            CancellationToken cancellationToken)
        {
            var userId = GetAuthenticatedUserId();

            var requestedTenantId = ReadOptionalId("X-Tenant-Id");
            var requestedCompanyId = ReadOptionalId("X-Company-Id");

            var isAdmin = await IsGlobalAdminAsync(context, userId, cancellationToken);

            if (isAdmin)
            {
                var company = requestedCompanyId is long requestedCompanyValue
                    ? await context.Empresas.FirstOrDefaultAsync(e => e.Id == requestedCompanyValue, cancellationToken)
                    : null;

                if (requestedCompanyId.HasValue && company is null)
                    throw new GraphQLException("Empresa do contexto não encontrada.");

                if (requestedTenantId.HasValue && !await context.Tenants
                    .AnyAsync(t => t.Id == requestedTenantId.Value, cancellationToken))
                    throw new GraphQLException("Tenant do contexto não encontrado.");

                if (company is not null && requestedTenantId.HasValue && company.TenantId != requestedTenantId.Value)
                    throw new GraphQLException("A empresa não pertence ao tenant informado.");

                return new ActiveTenantContext(
                    userId,
                    requestedTenantId ?? company?.TenantId,
                    requestedCompanyId,
                    IsAdmin: true);
            }

            var tenantIds = await context.UsuariosTenants
                .Where(m => m.UsuarioId == userId && m.Ativo)
                .Select(m => m.TenantId)
                .ToArrayAsync(cancellationToken);

            var tenantId = requestedTenantId;
            if (!tenantId.HasValue)
            {
                if (tenantIds.Length != 1)
                    throw new GraphQLException("Informe um tenant autorizado no contexto da requisição.");

                tenantId = tenantIds[0];
            }

            if (!tenantIds.Contains(tenantId.Value))
                throw new GraphQLException("Acesso não autorizado ao tenant informado.");

            var companyIds = await context.UsuariosEmpresas
                .Where(m => m.UsuarioId == userId
                    && m.Ativo
                    && m.Empresa.Ativo
                    && m.Empresa.TenantId == tenantId.Value)
                .Select(m => m.EmpresaId)
                .ToArrayAsync(cancellationToken);

            var companyId = requestedCompanyId;
            if (!companyId.HasValue && companyIds.Length == 1)
                companyId = companyIds[0];

            if (companyId.HasValue && !companyIds.Contains(companyId.Value))
                throw new GraphQLException("Acesso não autorizado à empresa informada.");

            var activeContext = new ActiveTenantContext(userId, tenantId, companyId, IsAdmin: false);
            await permissionAuthorizationService.RequireAsync(
                context, activeContext, PermissionCodes.TenantSwitch, cancellationToken);

            return activeContext;
        }

        public long GetAuthenticatedUserId()
        {
            var principal = httpContextAccessor.HttpContext?.User;
            if (principal?.Identity?.IsAuthenticated != true)
                throw new GraphQLException("Autenticação obrigatória.");

            var userIdValue = principal.FindFirstValue(ClaimTypes.NameIdentifier)
                ?? principal.FindFirstValue("sub");
            if (!long.TryParse(userIdValue, NumberStyles.None, CultureInfo.InvariantCulture, out var userId))
                throw new GraphQLException("Identidade autenticada inválida.");

            return userId;
        }

        public Task<bool> IsGlobalAdminAsync(
            Context context,
            long userId,
            CancellationToken cancellationToken) => (
                from userRole in context.UserRoles
                join role in context.Roles on userRole.RoleId equals role.Id
                where userRole.UserId == userId && role.NormalizedName == "ADMIN"
                select userRole.UserId
            ).AnyAsync(cancellationToken);

        public async Task EnsureCanAccessCompanyAsync(
            Context context,
            ActiveTenantContext activeContext,
            long companyId,
            CancellationToken cancellationToken)
        {
            var company = await context.Empresas
                .FirstOrDefaultAsync(e => e.Id == companyId, cancellationToken);

            if (company is null
                || (!activeContext.IsAdmin && activeContext.TenantId.HasValue && company.TenantId != activeContext.TenantId.Value))
                throw new GraphQLException("Empresa fora do contexto ativo.");

            if (!activeContext.IsAdmin)
            {
                if (activeContext.CompanyId is not long activeCompanyId)
                    throw new GraphQLException("Selecione uma empresa no contexto da requisição.");

                if (activeCompanyId != companyId)
                    throw new GraphQLException("A empresa informada não corresponde à empresa ativa.");

                if (!await context.UsuariosEmpresas
                    .AnyAsync(m => m.UsuarioId == activeContext.UserId
                    && m.EmpresaId == companyId
                    && m.Ativo,
                    cancellationToken))
                    throw new GraphQLException("Acesso não autorizado à empresa.");
            }
        }

        private long? ReadOptionalId(string headerName)
        {
            var value = httpContextAccessor.HttpContext?.Request.Headers[headerName].ToString();
            if (string.IsNullOrWhiteSpace(value))
                return null;

            if (!long.TryParse(value, NumberStyles.None, CultureInfo.InvariantCulture, out var id) || id <= 0)
                throw new GraphQLException($"O header {headerName} deve conter um identificador positivo.");

            return id;
        }
    }
}
