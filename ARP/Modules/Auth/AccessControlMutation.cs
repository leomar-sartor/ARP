using ARP.Entity.Cadastros;
using ARP.Infra;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace ARP.Modules.Auth
{
    [ExtendObjectType("Mutation")]
    public sealed class AccessControlMutation
    {
        private readonly ILogger<AccessControlMutation> _logger;

        public AccessControlMutation(ILogger<AccessControlMutation> logger)
        {
            _logger = logger;
        }

        public async Task<Tenant> CreateTenant(
            string nome,
            [Service] Context context,
            [Service] TenantContextResolver tenantContextResolver,
            CancellationToken cancellationToken)
        {
            var activeContext = await RequireAdminAsync(context, tenantContextResolver, cancellationToken);
            if (string.IsNullOrWhiteSpace(nome))
                throw new GraphQLException("O nome do tenant é obrigatório.");
            var tenant = new Tenant { Nome = nome.Trim() };
            context.Tenants.Add(tenant);
            await context.SaveChangesAsync(cancellationToken);
            _logger.LogWarning("Admin {AdminUserId} created tenant {TenantId}", activeContext.UserId, tenant.Id);
            return tenant;
        }

        public async Task<Tenant?> UpdateTenant(long id, string nome, [Service] Context context,
            [Service] TenantContextResolver resolver, CancellationToken cancellationToken)
        {
            var admin = await RequireAdminAsync(context, resolver, cancellationToken);
            if (string.IsNullOrWhiteSpace(nome)) throw new GraphQLException("O nome do tenant é obrigatório.");
            var tenant = await context.Tenants.FirstOrDefaultAsync(t => t.Id == id, cancellationToken);
            if (tenant is null) return null;
            tenant.Nome = nome.Trim(); tenant.UserId = admin.UserId; tenant.UpdatedAt = DateTime.UtcNow;
            await context.SaveChangesAsync(cancellationToken);
            return tenant;
        }

        public async Task<bool> RemoveTenant(long id, [Service] Context context,
            [Service] TenantContextResolver resolver, CancellationToken cancellationToken)
        {
            var admin = await RequireAdminAsync(context, resolver, cancellationToken);
            var tenant = await context.Tenants.FirstOrDefaultAsync(t => t.Id == id, cancellationToken);
            if (tenant is null) return false;
            if (await context.Empresas.AnyAsync(e => e.TenantId == id, cancellationToken)
                || await context.UsuariosTenants.AnyAsync(m => m.TenantId == id && m.Ativo, cancellationToken)
                || await context.UsuariosPersonas.AnyAsync(m => m.TenantId == id && m.Ativo, cancellationToken))
                throw new GraphQLException("Não é possível remover um tenant com empresas ou usuários vinculados.");
            tenant.Ativo = false; tenant.DeletedAt = DateTime.UtcNow; tenant.UpdatedAt = DateTime.UtcNow; tenant.UserId = admin.UserId;
            await context.SaveChangesAsync(cancellationToken);
            return true;
        }

        public async Task<bool> SetPersonaPermissions(
            string personaCode,
            string[] permissionCodes,
            [Service] Context context,
            [Service] TenantContextResolver tenantContextResolver,
            CancellationToken cancellationToken)
        {
            var activeContext = await RequireAdminAsync(context, tenantContextResolver, cancellationToken);
            var persona = await context.Personas
                .FirstOrDefaultAsync(item => item.Codigo == personaCode && item.Ativo, cancellationToken)
                ?? throw new GraphQLException("Persona não encontrada.");

            if (persona.Codigo == PersonaCodes.Admin)
                throw new GraphQLException("As permissões globais do Admin não são configuráveis por persona.");

            var codes = permissionCodes.Distinct(StringComparer.Ordinal).ToArray();
            var permissions = await context.Permissoes
                .Where(permission => codes.Contains(permission.Codigo) && permission.Ativo)
                .ToListAsync(cancellationToken);

            if (permissions.Count != codes.Length)
                throw new GraphQLException("Uma ou mais permissões informadas não existem ou estão inativas.");

            if (permissions.Any(permission =>
                !PermissionAuthorizationService.CanAssignToPersona(persona.Codigo, permission.Codigo)))
                throw new GraphQLException(
                    "A configuração ultrapassa os limites obrigatórios de segurança da persona.");

            var currentMappings = await context.PersonasPermissoes
                .Where(mapping => mapping.PersonaId == persona.Id)
                .ToListAsync(cancellationToken);
            var desiredIds = permissions.Select(permission => permission.Id).ToHashSet();
            var existingIds = currentMappings.Select(mapping => mapping.PermissaoId).ToHashSet();
            context.PersonasPermissoes.RemoveRange(currentMappings.Where(mapping => !desiredIds.Contains(mapping.PermissaoId)));
            foreach (var permission in permissions.Where(permission => !existingIds.Contains(permission.Id)))
            {
                context.PersonasPermissoes.Add(new PersonaPermissao
                {
                    PersonaId = persona.Id,
                    PermissaoId = permission.Id
                });
            }

            await context.SaveChangesAsync(cancellationToken);
            _logger.LogWarning(
                "Admin {AdminUserId} updated permissions for persona {PersonaCode}",
                activeContext.UserId,
                persona.Codigo);
            return true;
        }

        public async Task<UsuarioPersona> AssignPersona(
            long userId,
            string personaCode,
            long tenantId,
            long? companyId,
            [Service] Context context,
            [Service] TenantContextResolver tenantContextResolver,
            CancellationToken cancellationToken)
        {
            var activeContext = await RequireAdminAsync(context, tenantContextResolver, cancellationToken);
            var persona = await context.Personas
                .FirstOrDefaultAsync(item => item.Codigo == personaCode && item.Ativo, cancellationToken)
                ?? throw new GraphQLException("Persona não encontrada.");

            if (persona.Codigo == PersonaCodes.Admin)
                throw new GraphQLException("A persona Admin é global e deve ser concedida pelo fluxo de administração global.");

            var hasTenantMembership = await context.UsuariosTenants
                .AnyAsync(membership => membership.UsuarioId == userId
                    && membership.TenantId == tenantId
                    && membership.Ativo,
                    cancellationToken);
            if (!hasTenantMembership)
                throw new GraphQLException("O usuário não possui vínculo ativo com o tenant informado.");

            if (companyId is long selectedCompanyId)
            {
                var hasCompanyMembership = await context.UsuariosEmpresas
                    .AnyAsync(membership => membership.UsuarioId == userId
                        && membership.EmpresaId == selectedCompanyId
                        && membership.Empresa.TenantId == tenantId
                        && membership.Ativo,
                        cancellationToken);
                if (!hasCompanyMembership)
                    throw new GraphQLException("O usuário não possui vínculo ativo com a empresa informada.");
            }

            var assignment = await context.UsuariosPersonas
                .FirstOrDefaultAsync(item => item.UsuarioId == userId
                    && item.PersonaId == persona.Id
                    && item.TenantId == tenantId
                    && item.EmpresaId == companyId,
                    cancellationToken);

            if (assignment is null)
            {
                assignment = new UsuarioPersona
                {
                    UsuarioId = userId,
                    PersonaId = persona.Id,
                    TenantId = tenantId,
                    EmpresaId = companyId,
                    UserId = activeContext.UserId
                };
                context.UsuariosPersonas.Add(assignment);
            }
            else
            {
                assignment.Ativo = true;
                assignment.DeletedAt = null;
                assignment.UserId = activeContext.UserId;
                assignment.UpdatedAt = DateTime.UtcNow;
            }

            await context.SaveChangesAsync(cancellationToken);
            _logger.LogWarning(
                "Admin {AdminUserId} assigned persona {PersonaCode} to user {TargetUserId} in tenant {TenantId}, company {CompanyId}",
                activeContext.UserId,
                persona.Codigo,
                userId,
                tenantId,
                companyId);
            return assignment;
        }

        public async Task<UsuarioTenant> GrantTenantMembership(
            long userId,
            long tenantId,
            [Service] Context context,
            [Service] TenantContextResolver tenantContextResolver,
            CancellationToken cancellationToken)
        {
            var activeContext = await RequireAdminAsync(context, tenantContextResolver, cancellationToken);
            if (!await context.Users.AnyAsync(user => user.Id == userId, cancellationToken))
                throw new GraphQLException("Usuário não encontrado.");
            if (!await context.Tenants.AnyAsync(tenant => tenant.Id == tenantId, cancellationToken))
                throw new GraphQLException("Tenant não encontrado.");

            var membership = await context.UsuariosTenants
                .FirstOrDefaultAsync(item => item.UsuarioId == userId && item.TenantId == tenantId, cancellationToken);
            if (membership is null)
            {
                membership = new UsuarioTenant
                {
                    UsuarioId = userId,
                    TenantId = tenantId,
                    UserId = activeContext.UserId
                };
                context.UsuariosTenants.Add(membership);
            }
            else
            {
                membership.Ativo = true;
                membership.DeletedAt = null;
                membership.UserId = activeContext.UserId;
                membership.UpdatedAt = DateTime.UtcNow;
            }

            await context.SaveChangesAsync(cancellationToken);
            _logger.LogWarning("Admin {AdminUserId} granted tenant {TenantId} to user {TargetUserId}",
                activeContext.UserId, tenantId, userId);
            return membership;
        }

        public async Task<bool> RevokeTenantMembership(long userId, long tenantId,
            [Service] Context context, [Service] TenantContextResolver resolver, CancellationToken cancellationToken)
        {
            var admin = await RequireAdminAsync(context, resolver, cancellationToken);
            var membership = await context.UsuariosTenants.FirstOrDefaultAsync(m =>
                m.UsuarioId == userId && m.TenantId == tenantId && m.Ativo, cancellationToken);
            if (membership is null) return false;
            await DeactivateTenantMembership(context, membership, admin.UserId, cancellationToken);
            await context.SaveChangesAsync(cancellationToken);
            return true;
        }

        public async Task<bool> UpdateTenantMembership(long userId, long tenantId, long newTenantId,
            [Service] Context context, [Service] TenantContextResolver resolver, CancellationToken cancellationToken)
        {
            var admin = await RequireAdminAsync(context, resolver, cancellationToken);
            var membership = await context.UsuariosTenants.FirstOrDefaultAsync(m =>
                m.UsuarioId == userId && m.TenantId == tenantId && m.Ativo, cancellationToken)
                ?? throw new GraphQLException("Vínculo com o tenant não encontrado.");
            if (tenantId == newTenantId) return true;
            if (!await context.Tenants.AnyAsync(t => t.Id == newTenantId && t.Ativo, cancellationToken))
                throw new GraphQLException("Tenant de destino não encontrado.");
            var destination = await context.UsuariosTenants.FirstOrDefaultAsync(m =>
                m.UsuarioId == userId && m.TenantId == newTenantId, cancellationToken);
            if (destination?.Ativo == true)
                throw new GraphQLException("Este usuário já pertence ao tenant de destino.");
            if (destination is null)
                context.UsuariosTenants.Add(new UsuarioTenant { UsuarioId = userId, TenantId = newTenantId, UserId = admin.UserId });
            else
            {
                destination.Ativo = true; destination.DeletedAt = null;
                destination.UserId = admin.UserId; destination.UpdatedAt = DateTime.UtcNow;
            }
            await DeactivateTenantMembership(context, membership, admin.UserId, cancellationToken);
            // One SaveChanges commits the new membership and revocations atomically.
            await context.SaveChangesAsync(cancellationToken);
            return true;
        }

        private static async Task DeactivateTenantMembership(Context context, UsuarioTenant membership,
            long adminId, CancellationToken cancellationToken)
        {
            membership.Ativo = false; membership.UserId = adminId; membership.UpdatedAt = DateTime.UtcNow;
            var companies = await context.UsuariosEmpresas.Where(m => m.UsuarioId == membership.UsuarioId
                && m.Empresa.TenantId == membership.TenantId && m.Ativo).ToListAsync(cancellationToken);
            foreach (var company in companies)
            {
                company.Ativo = false; company.UserId = adminId; company.UpdatedAt = DateTime.UtcNow;
            }
            var personas = await context.UsuariosPersonas.Where(m => m.UsuarioId == membership.UsuarioId
                && m.TenantId == membership.TenantId && m.Ativo).ToListAsync(cancellationToken);
            foreach (var persona in personas)
            {
                persona.Ativo = false; persona.UserId = adminId; persona.UpdatedAt = DateTime.UtcNow;
            }
        }

        public async Task<UsuarioEmpresa> GrantCompanyMembership(
            long userId,
            long companyId,
            [Service] Context context,
            [Service] TenantContextResolver tenantContextResolver,
            CancellationToken cancellationToken)
        {
            var activeContext = await RequireAdminAsync(context, tenantContextResolver, cancellationToken);
            var company = await context.Empresas
                .FirstOrDefaultAsync(item => item.Id == companyId && item.Ativo, cancellationToken)
                ?? throw new GraphQLException("Empresa não encontrada ou inativa.");
            if (!await context.Users.AnyAsync(user => user.Id == userId, cancellationToken))
                throw new GraphQLException("Usuário não encontrado.");

            var tenantMembership = await context.UsuariosTenants
                .FirstOrDefaultAsync(item => item.UsuarioId == userId
                    && item.TenantId == company.TenantId,
                    cancellationToken);
            if (tenantMembership is null)
            {
                tenantMembership = new UsuarioTenant
                {
                    UsuarioId = userId,
                    TenantId = company.TenantId,
                    UserId = activeContext.UserId
                };
                context.UsuariosTenants.Add(tenantMembership);
            }
            else
            {
                tenantMembership.Ativo = true;
                tenantMembership.DeletedAt = null;
                tenantMembership.UserId = activeContext.UserId;
                tenantMembership.UpdatedAt = DateTime.UtcNow;
            }

            var membership = await context.UsuariosEmpresas
                .FirstOrDefaultAsync(item => item.UsuarioId == userId && item.EmpresaId == companyId, cancellationToken);
            if (membership is null)
            {
                membership = new UsuarioEmpresa
                {
                    UsuarioId = userId,
                    EmpresaId = companyId,
                    UserId = activeContext.UserId
                };
                context.UsuariosEmpresas.Add(membership);
            }
            else
            {
                membership.Ativo = true;
                membership.DeletedAt = null;
                membership.UserId = activeContext.UserId;
                membership.UpdatedAt = DateTime.UtcNow;
            }

            await context.SaveChangesAsync(cancellationToken);
            _logger.LogWarning("Admin {AdminUserId} granted company {CompanyId} to user {TargetUserId}",
                activeContext.UserId, companyId, userId);
            return membership;
        }

        public async Task<bool> RevokeCompanyMembership(
            long userId,
            long companyId,
            [Service] Context context,
            [Service] TenantContextResolver tenantContextResolver,
            CancellationToken cancellationToken)
        {
            var activeContext = await RequireAdminAsync(context, tenantContextResolver, cancellationToken);
            var membership = await context.UsuariosEmpresas
                .FirstOrDefaultAsync(item => item.UsuarioId == userId && item.EmpresaId == companyId, cancellationToken);
            if (membership is null)
                return false;

            membership.Ativo = false;
            membership.UserId = activeContext.UserId;
            membership.UpdatedAt = DateTime.UtcNow;

            var companyScopedAssignments = await context.UsuariosPersonas
                .Where(item => item.UsuarioId == userId && item.EmpresaId == companyId && item.Ativo)
                .ToListAsync(cancellationToken);
            foreach (var assignment in companyScopedAssignments)
            {
                assignment.Ativo = false;
                assignment.UserId = activeContext.UserId;
                assignment.UpdatedAt = DateTime.UtcNow;
            }

            await context.SaveChangesAsync(cancellationToken);
            _logger.LogWarning("Admin {AdminUserId} revoked company {CompanyId} from user {TargetUserId}",
                activeContext.UserId, companyId, userId);
            return true;
        }

        public async Task<bool> RevokePersona(
            long assignmentId,
            [Service] Context context,
            [Service] TenantContextResolver tenantContextResolver,
            CancellationToken cancellationToken)
        {
            var activeContext = await RequireAdminAsync(context, tenantContextResolver, cancellationToken);
            var assignment = await context.UsuariosPersonas
                .FirstOrDefaultAsync(item => item.Id == assignmentId, cancellationToken);
            if (assignment is null)
                return false;

            assignment.Ativo = false;
            assignment.UpdatedAt = DateTime.UtcNow;
            assignment.UserId = activeContext.UserId;
            await context.SaveChangesAsync(cancellationToken);
            _logger.LogWarning(
                "Admin {AdminUserId} revoked contextual persona assignment {AssignmentId}",
                activeContext.UserId,
                assignmentId);
            return true;
        }

        public async Task<bool> GrantGlobalAdmin(
            long userId,
            [Service] Context context,
            [Service] TenantContextResolver tenantContextResolver,
            [Service] UserManager<Usuario> userManager,
            [Service] RoleManager<IdentityRole<long>> roleManager,
            CancellationToken cancellationToken)
        {
            var activeContext = await RequireAdminAsync(context, tenantContextResolver, cancellationToken);

            var user = await userManager.FindByIdAsync(userId.ToString())
                ?? throw new GraphQLException("Usuário não encontrado.");
            var role = await roleManager.FindByNameAsync("Admin");
            if (role is null)
            {
                var createRoleResult = await roleManager.CreateAsync(new IdentityRole<long>("Admin"));
                if (!createRoleResult.Succeeded)
                    throw new GraphQLException("Não foi possível criar a role global de Admin.");
                role = await roleManager.FindByNameAsync("Admin");
            }

            if (role is null)
                throw new GraphQLException("Role global de Admin indisponível.");

            if (await userManager.IsInRoleAsync(user, role.Name!)) return true;
            var result = await userManager.AddToRoleAsync(user, role.Name!);
            if (!result.Succeeded)
                throw new GraphQLException("Não foi possível conceder acesso global de Admin.");

            _logger.LogWarning(
                "Admin {AdminUserId} granted global Admin to user {TargetUserId}",
                activeContext.UserId,
                userId);
            return true;
        }

        public async Task<bool> RevokeGlobalAdmin(
            long userId,
            [Service] Context context,
            [Service] TenantContextResolver tenantContextResolver,
            [Service] UserManager<Usuario> userManager,
            [Service] RoleManager<IdentityRole<long>> roleManager,
            CancellationToken cancellationToken)
        {
            var activeContext = await RequireAdminAsync(context, tenantContextResolver, cancellationToken);
            if (activeContext.UserId == userId)
                throw new GraphQLException("Admin não pode revogar o próprio acesso global.");

            var role = await roleManager.FindByNameAsync("Admin");
            if (role is null)
                return false;

            var user = await userManager.FindByIdAsync(userId.ToString())
                ?? throw new GraphQLException("Usuário não encontrado.");
            if (!await userManager.IsInRoleAsync(user, role.Name!)) return false;
            var adminsCount = await context.UserRoles
                .CountAsync(userRole => userRole.RoleId == role.Id, cancellationToken);
            if (adminsCount <= 1)
                throw new GraphQLException("Não é permitido remover o último Admin global.");

            var result = await userManager.RemoveFromRoleAsync(user, role.Name!);
            if (!result.Succeeded)
                throw new GraphQLException("Não foi possível revogar o acesso global de Admin.");

            _logger.LogWarning(
                "Admin {AdminUserId} revoked global Admin from user {TargetUserId}",
                activeContext.UserId,
                userId);
            return true;
        }

        public async Task<bool> UpdateAccessUser(long userId, string userName, string email,
            [Service] Context context, [Service] TenantContextResolver resolver,
            [Service] UserManager<Usuario> userManager, CancellationToken cancellationToken)
        {
            await RequireAdminAsync(context, resolver, cancellationToken);
            var user = await userManager.FindByIdAsync(userId.ToString())
                ?? throw new GraphQLException("Usuário não encontrado.");
            if (string.IsNullOrWhiteSpace(userName) || string.IsNullOrWhiteSpace(email))
                throw new GraphQLException("Nome de usuário e e-mail são obrigatórios.");
            var existing = await userManager.FindByEmailAsync(email.Trim());
            if (existing is not null && existing.Id != userId)
                throw new GraphQLException("Este e-mail já está em uso.");
            if (!string.Equals(user.Email, email.Trim(), StringComparison.OrdinalIgnoreCase))
                user.EmailConfirmed = false;
            user.Email = email.Trim();
            user.UserName = userName.Trim();
            var result = await userManager.UpdateAsync(user);
            if (!result.Succeeded)
                throw new GraphQLException(string.Join(" ", result.Errors.Select(e => e.Description)));
            return true;
        }

        public async Task<bool> RemoveAccessUser(long userId,
            [Service] Context context, [Service] TenantContextResolver resolver,
            [Service] UserManager<Usuario> userManager, CancellationToken cancellationToken)
        {
            var admin = await RequireAdminAsync(context, resolver, cancellationToken);
            if (admin.UserId == userId) throw new GraphQLException("Não é possível excluir sua própria conta.");
            if (await resolver.IsGlobalAdminAsync(context, userId, cancellationToken))
                throw new GraphQLException("Revogue o acesso Admin antes de excluir este usuário.");
            var user = await userManager.FindByIdAsync(userId.ToString())
                ?? throw new GraphQLException("Usuário não encontrado.");
            if (user.EmpresaId is not null
                || await context.UsuariosTenants.IgnoreQueryFilters().AnyAsync(m => m.UsuarioId == userId, cancellationToken)
                || await context.UsuariosEmpresas.IgnoreQueryFilters().AnyAsync(m => m.UsuarioId == userId, cancellationToken)
                || await context.UsuariosPersonas.IgnoreQueryFilters().AnyAsync(m => m.UsuarioId == userId, cancellationToken)
                || await context.Colaboradores.IgnoreQueryFilters().AnyAsync(c => c.UsuarioId == userId, cancellationToken))
                throw new GraphQLException("Este usuário possui vínculos ou histórico e não pode ser excluído.");
            var result = await userManager.DeleteAsync(user);
            if (!result.Succeeded) throw new GraphQLException("Não foi possível excluir o usuário.");
            return true;
        }

        public async Task<bool> UpdateAccessPermission(long id, string descricao,
            [Service] Context context, [Service] TenantContextResolver resolver, CancellationToken cancellationToken)
        {
            var admin = await RequireAdminAsync(context, resolver, cancellationToken);
            if (string.IsNullOrWhiteSpace(descricao)) throw new GraphQLException("Informe a descrição da permissão.");
            var permission = await context.Permissoes.FirstOrDefaultAsync(p => p.Id == id, cancellationToken)
                ?? throw new GraphQLException("Permissão não encontrada.");
            permission.Descricao = descricao.Trim(); permission.UpdatedAt = DateTime.UtcNow; permission.UserId = admin.UserId;
            await context.SaveChangesAsync(cancellationToken);
            return true;
        }

        public async Task<bool> RemoveAccessPermission(long id,
            [Service] Context context, [Service] TenantContextResolver resolver, CancellationToken cancellationToken)
        {
            await RequireAdminAsync(context, resolver, cancellationToken);
            var permission = await context.Permissoes.FirstOrDefaultAsync(p => p.Id == id, cancellationToken)
                ?? throw new GraphQLException("Permissão não encontrada.");
            var systemCodes = typeof(PermissionCodes).GetFields().Select(f => f.GetRawConstantValue() as string);
            if (systemCodes.Contains(permission.Codigo))
                throw new GraphQLException("Permissões do sistema não podem ser excluídas. Revogue sua atribuição na persona.");
            if (await context.PersonasPermissoes.AnyAsync(p => p.PermissaoId == id, cancellationToken))
                throw new GraphQLException("Revogue as atribuições desta permissão antes de excluí-la.");
            context.Permissoes.Remove(permission);
            await context.SaveChangesAsync(cancellationToken);
            return true;
        }

        private static async Task<ActiveTenantContext> RequireAdminAsync(
            Context context,
            TenantContextResolver tenantContextResolver,
            CancellationToken cancellationToken)
        {
            var activeContext = await tenantContextResolver.ResolveAsync(context, cancellationToken);
            if (!activeContext.IsAdmin)
                throw new GraphQLException(
                    ErrorBuilder.New()
                        .SetMessage("Somente Admin pode administrar personas e permissões.")
                        .SetCode("AUTH_NOT_AUTHORIZED")
                        .Build());

            return activeContext;
        }
    }
}
