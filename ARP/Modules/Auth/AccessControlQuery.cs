using ARP.Entity.Cadastros;
using ARP.Infra;
using Microsoft.EntityFrameworkCore;

namespace ARP.Modules.Auth
{
    public sealed record AvailableAccessContext(
        long? TenantId,
        string TenantName,
        long? CompanyId,
        string? CompanyName);

    public sealed record AvailableAccessContexts(
        bool IsAdmin,
        IReadOnlyList<AvailableAccessContext> Contexts);

    public sealed record AccessManagedUser(long Id, string? Email, string? UserName, bool Ativo, bool IsAdmin, long[] CompanyIds);

    public sealed record AccessManagedCompany(long Id, string Name);
    public sealed record AccessUserTenant(long Id, long TenantId, string TenantName);

    public sealed record AccessPersonaAssignment(
        long Id,
        long UserId,
        string? Email,
        string? UserName,
        string PersonaCode,
        long TenantId,
        string TenantName,
        long? CompanyId,
        string? CompanyName);

    [ExtendObjectType("Query")]
    public sealed class AccessControlQuery
    {
        public async Task<IReadOnlyList<AccessUserTenant>> GetAccessUserTenants(long userId,
            [Service] Context context, [Service] TenantContextResolver resolver, CancellationToken cancellationToken)
        {
            EnsureAdmin(await resolver.ResolveAsync(context, cancellationToken));
            return await context.UsuariosTenants.Where(m => m.UsuarioId == userId && m.Ativo)
                .OrderBy(m => m.Tenant.Nome)
                .Select(m => new AccessUserTenant(m.Id, m.TenantId, m.Tenant.Nome))
                .ToArrayAsync(cancellationToken);
        }

        public async Task<IReadOnlyList<AccessManagedUser>> GetAccessManagedUsers(
            [Service] Context context,
            [Service] TenantContextResolver tenantContextResolver,
            CancellationToken cancellationToken)
        {
            var activeContext = await tenantContextResolver.ResolveAsync(context, cancellationToken);
            EnsureAdmin(activeContext);

            return await context.Users
                .OrderBy(user => user.UserName)
                .Select(user => new AccessManagedUser(
                    user.Id,
                    user.Email,
                    user.UserName,
                    user.Ativo,
                    context.UserRoles.Any(ur => ur.UserId == user.Id && context.Roles.Any(r => r.Id == ur.RoleId && r.NormalizedName == "ADMIN")),
                    context.UsuariosEmpresas.Where(m => m.UsuarioId == user.Id && m.Ativo).Select(m => m.EmpresaId).ToArray()))
                .ToArrayAsync(cancellationToken);
        }

        public async Task<IReadOnlyList<AccessManagedCompany>> GetTenantCompaniesForAccessManagement(
            long tenantId,
            [Service] Context context,
            [Service] TenantContextResolver tenantContextResolver,
            CancellationToken cancellationToken)
        {
            var activeContext = await tenantContextResolver.ResolveAsync(context, cancellationToken);
            EnsureAdmin(activeContext);

            return await context.Empresas
                .Where(company => company.TenantId == tenantId && company.Ativo)
                .OrderBy(company => company.NomeFantasia)
                .Select(company => new AccessManagedCompany(
                    company.Id,
                    company.NomeFantasia ?? company.Cnpj))
                .ToArrayAsync(cancellationToken);
        }

        public async Task<IReadOnlyList<AccessPersonaAssignment>> GetActivePersonaAssignments(
            [Service] Context context,
            [Service] TenantContextResolver tenantContextResolver,
            CancellationToken cancellationToken)
        {
            var activeContext = await tenantContextResolver.ResolveAsync(context, cancellationToken);
            EnsureAdmin(activeContext);

            return await context.UsuariosPersonas
                .Where(assignment => assignment.Ativo)
                .OrderBy(assignment => assignment.Usuario.UserName)
                .Select(assignment => new AccessPersonaAssignment(
                    assignment.Id,
                    assignment.UsuarioId,
                    assignment.Usuario.Email,
                    assignment.Usuario.UserName,
                    assignment.Persona.Codigo,
                    assignment.TenantId,
                    assignment.Tenant.Nome,
                    assignment.EmpresaId,
                    assignment.Empresa == null
                        ? null
                        : assignment.Empresa.NomeFantasia ?? assignment.Empresa.Cnpj))
                .ToArrayAsync(cancellationToken);
        }

        public async Task<AvailableAccessContexts> GetMyAvailableAccessContexts(
            [Service] Context context,
            [Service] TenantContextResolver tenantContextResolver,
            CancellationToken cancellationToken)
        {
            var userId = tenantContextResolver.GetAuthenticatedUserId();
            var isAdmin = await tenantContextResolver.IsGlobalAdminAsync(
                context, userId, cancellationToken);

            var tenants = await context.Tenants
                .Where(tenant => isAdmin || context.UsuariosTenants.Any(membership =>
                    membership.UsuarioId == userId
                    && membership.TenantId == tenant.Id
                    && membership.Ativo))
                .OrderBy(tenant => tenant.Nome)
                .Select(tenant => new { tenant.Id, tenant.Nome })
                .ToListAsync(cancellationToken);

            var companies = await context.Empresas
                .Where(company => company.Ativo && (isAdmin || context.UsuariosEmpresas.Any(membership =>
                    membership.UsuarioId == userId
                    && membership.EmpresaId == company.Id
                    && membership.Ativo
                    && context.UsuariosTenants.Any(tenantMembership =>
                        tenantMembership.UsuarioId == userId
                        && tenantMembership.TenantId == company.TenantId
                        && tenantMembership.Ativo))))
                .OrderBy(company => company.NomeFantasia)
                .Select(company => new
                {
                    company.Id,
                    company.TenantId,
                    Name = company.NomeFantasia ?? company.Cnpj
                })
                .ToListAsync(cancellationToken);

            var availableContexts = tenants.SelectMany(tenant =>
            {
                var tenantCompanies = companies
                    .Where(company => company.TenantId == tenant.Id)
                    .ToArray();

                return tenantCompanies.Length == 0
                    ? [new AvailableAccessContext(tenant.Id, tenant.Nome, null, null)]
                    : tenantCompanies.Select(company => new AvailableAccessContext(
                        tenant.Id,
                        tenant.Nome,
                        company.Id,
                        company.Name));
            }).ToArray();

            // A tenant membership can remain after a contextual persona is revoked.
            if (!isAdmin)
            {
                var assignments = await context.UsuariosPersonas
                    .Where(a => a.UsuarioId == userId && a.Ativo && a.Persona.Ativo
                        && (a.Persona.Codigo == PersonaCodes.Operator || a.Persona.Codigo == PersonaCodes.Collaborator || a.Persona.Codigo == PersonaCodes.Company)
                        && a.Persona.Permissoes.Any(m => m.Permissao.Ativo && m.Permissao.Codigo == PermissionCodes.TenantSwitch))
                    .Select(a => new { a.TenantId, a.EmpresaId }).ToArrayAsync(cancellationToken);
                availableContexts = availableContexts.Where(c => assignments.Any(a =>
                    a.TenantId == c.TenantId && (a.EmpresaId == null || a.EmpresaId == c.CompanyId))).ToArray();
            }

            return new AvailableAccessContexts(isAdmin, isAdmin
                ? new[] { new AvailableAccessContext(null, "Administração global", null, null) }.Concat(availableContexts).ToArray()
                : availableContexts);
        }

        [UsePaging(IncludeTotalCount = true)]
        [UseFiltering]
        [UseSorting]
        public async Task<IQueryable<Tenant>> GetTenants([Service] Context context,
            [Service] TenantContextResolver resolver, CancellationToken cancellationToken)
        {
            EnsureAdmin(await resolver.ResolveAsync(context, cancellationToken));
            return context.Tenants;
        }

        public async Task<IReadOnlyList<Permissao>> GetAccessPermissions(
            [Service] Context context, [Service] TenantContextResolver resolver, CancellationToken cancellationToken)
        {
            EnsureAdmin(await resolver.ResolveAsync(context, cancellationToken));
            return await context.Permissoes.OrderBy(p => p.Codigo).ToArrayAsync(cancellationToken);
        }

        public async Task<IReadOnlyList<Persona>> GetAccessCatalog(
            [Service] Context context,
            [Service] TenantContextResolver tenantContextResolver,
            CancellationToken cancellationToken)
        {
            var activeContext = await tenantContextResolver.ResolveAsync(context, cancellationToken);
            EnsureAdmin(activeContext);

            return await context.Personas
                .Include(persona => persona.Permissoes)
                    .ThenInclude(mapping => mapping.Permissao)
                .OrderBy(persona => persona.Id)
                .ToListAsync(cancellationToken);
        }

        public async Task<IReadOnlyList<Permissao>> GetAssignablePermissions(
            string personaCode,
            [Service] Context context,
            [Service] TenantContextResolver tenantContextResolver,
            CancellationToken cancellationToken)
        {
            var activeContext = await tenantContextResolver.ResolveAsync(context, cancellationToken);
            EnsureAdmin(activeContext);

            var permissions = await context.Permissoes
                .Where(permission => permission.Ativo)
                .OrderBy(permission => permission.Codigo)
                .ToListAsync(cancellationToken);
            return permissions.Where(permission => PermissionAuthorizationService.CanAssignToPersona(personaCode, permission.Codigo)).ToArray();
        }

        public async Task<IReadOnlyList<string>> GetMyPersonas(
            [Service] Context context,
            [Service] TenantContextResolver tenantContextResolver,
            CancellationToken cancellationToken)
        {
            var activeContext = await tenantContextResolver.ResolveAsync(context, cancellationToken);
            if (activeContext.IsAdmin)
                return [PersonaCodes.Admin];

            if (activeContext.TenantId is not long tenantId)
                return Array.Empty<string>();

            return await context.UsuariosPersonas
                .Where(assignment => assignment.UsuarioId == activeContext.UserId
                    && assignment.TenantId == tenantId
                    && assignment.Ativo
                    && assignment.Persona.Ativo
                    && (assignment.EmpresaId == null || assignment.EmpresaId == activeContext.CompanyId))
                .Select(assignment => assignment.Persona.Codigo)
                .Distinct()
                .ToArrayAsync(cancellationToken);
        }

        public async Task<IReadOnlyList<string>> GetMyPermissions(
            [Service] Context context,
            [Service] TenantContextResolver tenantContextResolver,
            [Service] PermissionAuthorizationService permissionAuthorizationService,
            CancellationToken cancellationToken)
        {
            var activeContext = await tenantContextResolver.ResolveAsync(context, cancellationToken);
            var candidates = await context.Permissoes
                .Where(permission => permission.Ativo)
                .Select(permission => permission.Codigo)
                .ToArrayAsync(cancellationToken);

            var granted = new List<string>();
            foreach (var code in candidates)
            {
                if (await permissionAuthorizationService.CanAsync(
                    context, activeContext, code, cancellationToken))
                    granted.Add(code);
            }

            return granted;
        }

        private static void EnsureAdmin(ActiveTenantContext context)
        {
            if (!context.IsAdmin)
                throw new GraphQLException(
                    ErrorBuilder.New()
                        .SetMessage("Somente Admin pode consultar o catálogo de acesso.")
                        .SetCode("AUTH_NOT_AUTHORIZED")
                        .Build());
        }
    }
}
