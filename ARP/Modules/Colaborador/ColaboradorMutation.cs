using ARP.Infra;
using ARP.Entity.Cadastros;
using ARP.Modules.Auth;
using ARP.Modules.Colaborador.Types;
using ARP.Utils;
using Microsoft.EntityFrameworkCore;

namespace ARP.Modules.Colaborador
{
    [ExtendObjectType("Mutation")]
    public class ColaboradorMutation
    {
        private readonly ILogger<ColaboradorMutation> _logger;

        public ColaboradorMutation(
            ILogger<ColaboradorMutation> logger
            )
        {
            _logger = logger;
        }

        [GraphQLDescription("Cadastrar um novo colaborador")]
        public async Task<Entity.Cadastros.Colaborador> CreateColaborador(
        ColaboradorInput input,
        [Service] Context context,
        [Service] TenantContextResolver tenantContextResolver,
        [Service] PermissionAuthorizationService permissionAuthorizationService,
        CancellationToken ct)
        {
            var activeContext = await tenantContextResolver.ResolveAsync(context, ct);
            await permissionAuthorizationService.RequireAsync(
                context, activeContext, PermissionCodes.CollaboratorManage, ct);
            await tenantContextResolver.EnsureCanAccessCompanyAsync(
                context, activeContext, input.EmpresaId, ct);

            if (!CpfHelper.IsValidCpf(input.Cpf))
                throw new ArgumentException("CPF inválido");

            var cpf = CpfHelper.OnlyDigits(input.Cpf);

            var cpfAlreadyExists = await context.Colaboradores
                .AnyAsync(c => c.Cpf == cpf, ct);

            if (cpfAlreadyExists)
                throw new ArgumentException("Já existe um cadastro com este CPF.");

            var empresa = await context.Empresas
                .FirstOrDefaultAsync(e => e.Id == input.EmpresaId, ct);

            if (empresa == null)
                throw new ArgumentException("Empresa não encontrada");

            var setor = await context.Setores
                .FirstOrDefaultAsync(s => s.Id == input.SetorId && s.EmpresaId == input.EmpresaId, ct);

            if (setor == null)
                throw new ArgumentException("Setor não encontrado na empresa informada");

            var user = await context.Users.FirstOrDefaultAsync(account => account.Cpf == cpf, ct);
            if (user is not null && await context.Colaboradores
                .AnyAsync(existing => existing.UsuarioId == user.Id, ct))
                throw new ArgumentException("A conta já está associada a outro perfil de colaborador.");

            var entity = new Entity.Cadastros.Colaborador
            {
                Cpf = cpf,
                Nome = input.Nome,
                Email = input.Email,
                UsuarioId = user?.Id,
                SetorId = input.SetorId,
                EmpresaId = input.EmpresaId,
                EmpresasVinculadas =
                {
                    new Entity.Cadastros.ColaboradorEmpresa
                    {
                        EmpresaId = input.EmpresaId,
                        SetorId = input.SetorId
                    }
                }
            };

            context.Colaboradores.Add(entity);

            if (user is not null)
                await EnsureUserMembershipsAsync(context, user.Id, empresa, activeContext.UserId, ct);

            try
            {
                await context.SaveChangesAsync(ct);
            }
            catch (DbUpdateException ex) when (IsUniqueConstraintViolation(ex))
            {
                throw new ArgumentException("Já existe um cadastro com este CPF.");
            }

            return entity;
        }

        [GraphQLDescription("Vincular colaborador a uma empresa e setor autorizados")]
        public async Task<Entity.Cadastros.ColaboradorEmpresa> VincularColaboradorEmpresa(
            long colaboradorId,
            long empresaId,
            long setorId,
            [Service] Context context,
            [Service] TenantContextResolver tenantContextResolver,
            [Service] PermissionAuthorizationService permissionAuthorizationService,
            CancellationToken ct)
        {
            var activeContext = await tenantContextResolver.ResolveAsync(context, ct);
            await permissionAuthorizationService.RequireAsync(
                context, activeContext, PermissionCodes.CollaboratorManage, ct);
            await tenantContextResolver.EnsureCanAccessCompanyAsync(context, activeContext, empresaId, ct);

            var collaborator = await context.Colaboradores
                .FirstOrDefaultAsync(item => item.Id == colaboradorId, ct)
                ?? throw new GraphQLException("Colaborador não encontrado.");
            var company = await context.Empresas
                .FirstOrDefaultAsync(item => item.Id == empresaId && item.Ativo, ct)
                ?? throw new GraphQLException("Empresa não encontrada ou inativa.");
            var sector = await context.Setores
                .FirstOrDefaultAsync(item => item.Id == setorId
                    && item.EmpresaId == empresaId
                    && item.Ativo,
                    ct)
                ?? throw new GraphQLException("Setor não encontrado na empresa informada.");

            if (collaborator.UsuarioId is null)
            {
                var user = await context.Users.FirstOrDefaultAsync(account => account.Cpf == collaborator.Cpf, ct);
                if (user is not null)
                {
                    if (await context.Colaboradores.AnyAsync(item => item.UsuarioId == user.Id, ct))
                        throw new GraphQLException("A conta já está associada a outro perfil de colaborador.");
                    collaborator.UsuarioId = user.Id;
                }
            }

            var membership = await context.ColaboradorEmpresas
                .FirstOrDefaultAsync(item => item.ColaboradorId == colaboradorId && item.EmpresaId == empresaId, ct);
            if (membership is null)
            {
                membership = new Entity.Cadastros.ColaboradorEmpresa
                {
                    ColaboradorId = collaborator.Id,
                    EmpresaId = company.Id,
                    SetorId = sector.Id,
                    UserId = activeContext.UserId
                };
                context.ColaboradorEmpresas.Add(membership);
            }
            else
            {
                membership.SetorId = sector.Id;
                membership.Ativo = true;
                membership.UserId = activeContext.UserId;
                membership.UpdatedAt = DateTime.UtcNow;
            }

            if (collaborator.UsuarioId is long userId)
                await EnsureUserMembershipsAsync(context, userId, company, activeContext.UserId, ct);

            await context.SaveChangesAsync(ct);
            _logger.LogInformation("User {UserId} linked collaborator {CollaboratorId} to company {CompanyId}",
                activeContext.UserId, colaboradorId, empresaId);
            return membership;
        }

        [GraphQLDescription("Desvincular colaborador da empresa ativa")]
        public async Task<bool> DesvincularColaboradorEmpresa(
            long colaboradorId,
            [Service] Context context,
            [Service] TenantContextResolver tenantContextResolver,
            [Service] PermissionAuthorizationService permissionAuthorizationService,
            CancellationToken ct)
        {
            var activeContext = await tenantContextResolver.ResolveAsync(context, ct);
            await permissionAuthorizationService.RequireAsync(
                context, activeContext, PermissionCodes.CollaboratorManage, ct);
            var companyId = activeContext.CompanyId
                ?? throw new GraphQLException("Selecione uma empresa para remover o vínculo.");
            await tenantContextResolver.EnsureCanAccessCompanyAsync(context, activeContext, companyId, ct);

            var membership = await context.ColaboradorEmpresas
                .FirstOrDefaultAsync(item => item.ColaboradorId == colaboradorId
                    && item.EmpresaId == companyId
                    && item.Ativo,
                    ct);
            if (membership is null)
                return false;

            membership.Ativo = false;
            membership.UpdatedAt = DateTime.UtcNow;

            var collaborator = await context.Colaboradores
                .FirstOrDefaultAsync(item => item.Id == colaboradorId, ct);
            if (collaborator?.UsuarioId is long userId)
            {
                var collaboratorPersona = await context.UsuariosPersonas
                    .Where(assignment => assignment.UsuarioId == userId
                        && assignment.EmpresaId == companyId
                        && assignment.Persona.Codigo == PersonaCodes.Collaborator
                        && assignment.Ativo)
                    .ToListAsync(ct);
                foreach (var assignment in collaboratorPersona)
                {
                    assignment.Ativo = false;
                    assignment.UpdatedAt = DateTime.UtcNow;
                    assignment.UserId = activeContext.UserId;
                }
            }

            await context.SaveChangesAsync(ct);
            return true;
        }

        /// <summary>
        /// Updates an existing colaborador.
        /// </summary>
        [GraphQLDescription("Atualizar uma colaborador existente")]
        public async Task<Entity.Cadastros.Colaborador?> UpdateColaborador(
            long Id,
            ColaboradorInput input,
            [Service] Context context,
            [Service] TenantContextResolver tenantContextResolver,
            [Service] PermissionAuthorizationService permissionAuthorizationService,
            CancellationToken ct)
        {
            var activeContext = await tenantContextResolver.ResolveAsync(context, ct);
            await permissionAuthorizationService.RequireAsync(
                context, activeContext, PermissionCodes.CollaboratorManage, ct);
            var entity = await context.Colaboradores
                   .FirstOrDefaultAsync(p => p.Id == Id, ct);

            if (entity == null)
                return null;

            await tenantContextResolver.EnsureCanAccessCompanyAsync(
                context, activeContext, entity.EmpresaId, ct);
            await tenantContextResolver.EnsureCanAccessCompanyAsync(
                context, activeContext, input.EmpresaId, ct);

            if (!CpfHelper.IsValidCpf(input.Cpf))
                throw new ArgumentException("CPF inválido");

            var cpf = CpfHelper.OnlyDigits(input.Cpf);

            var cpfAlreadyExists = await context.Colaboradores
                .AnyAsync(c => c.Cpf == cpf && c.Id != Id, ct);

            if (cpfAlreadyExists)
                throw new ArgumentException("Já existe um cadastro com este CPF.");

            entity.Cpf = cpf;
            entity.Nome = input.Nome;
            entity.Email = input.Email;

            var empresa = await context.Empresas
                .FirstOrDefaultAsync(e => e.Id == input.EmpresaId, ct);
            if (empresa == null)
                throw new ArgumentException("Empresa não encontrada");

            var setor = await context.Setores
                .FirstOrDefaultAsync(s => s.Id == input.SetorId && s.EmpresaId == input.EmpresaId, ct);
            if (setor == null)
                throw new ArgumentException("Setor não encontrado na empresa informada");

            entity.SetorId = input.SetorId;
            entity.EmpresaId = input.EmpresaId;
            entity.UpdatedAt = DateTime.UtcNow;

            var membership = await context.ColaboradorEmpresas
                .FirstOrDefaultAsync(
                    x => x.ColaboradorId == entity.Id && x.EmpresaId == input.EmpresaId,
                    ct);

            if (membership == null)
            {
                context.ColaboradorEmpresas.Add(new Entity.Cadastros.ColaboradorEmpresa
                {
                    ColaboradorId = entity.Id,
                    EmpresaId = input.EmpresaId,
                    SetorId = input.SetorId
                });
            }
            else
            {
                membership.SetorId = input.SetorId;
                membership.Ativo = true;
                membership.UpdatedAt = DateTime.UtcNow;
            }

            try
            {
                await context.SaveChangesAsync(ct);
            }
            catch (DbUpdateException ex) when (IsUniqueConstraintViolation(ex))
            {
                throw new ArgumentException("Já existe um cadastro com este CPF.");
            }

            return entity;
        }
        [GraphQLDescription("Remover um colaborador")]
        public async Task<bool> RemoveColaborador(
            long id,
            [Service] Context context,
            [Service] TenantContextResolver tenantContextResolver,
            [Service] PermissionAuthorizationService permissionAuthorizationService,
            CancellationToken ct)
        {
            var activeContext = await tenantContextResolver.ResolveAsync(context, ct);
            await permissionAuthorizationService.RequireAsync(
                context, activeContext, PermissionCodes.CollaboratorManage, ct);
            await using var transaction = await context.Database.BeginTransactionAsync(ct);

            try
            {
                var entity = await context.Colaboradores
                .FirstOrDefaultAsync(p => p.Id == id, ct);

                if (entity == null)
                    return false;

                var now = DateTime.UtcNow;
                if (activeContext.IsAdmin)
                {
                    entity.DeletedAt = now;
                    foreach (var membership in await context.ColaboradorEmpresas
                        .Where(m => m.ColaboradorId == id && m.Ativo)
                        .ToListAsync(ct))
                    {
                        membership.Ativo = false;
                    }
                }
                else
                {
                    var companyId = activeContext.CompanyId
                        ?? throw new GraphQLException("Selecione uma empresa para remover o vínculo do colaborador.");
                    await tenantContextResolver.EnsureCanAccessCompanyAsync(
                        context, activeContext, companyId, ct);

                    var membership = await context.ColaboradorEmpresas
                        .FirstOrDefaultAsync(m => m.ColaboradorId == id
                            && m.EmpresaId == companyId
                            && m.Ativo,
                            ct);
                    if (membership is null)
                        throw new GraphQLException("O colaborador não está vinculado à empresa ativa.");

                    membership.Ativo = false;
                    membership.UpdatedAt = now;
                }

                await context.SaveChangesAsync(ct);

                await transaction.CommitAsync(ct);
            }
            catch
            {
                await transaction.RollbackAsync(ct);
                throw;
            }

            return true;
        }

        /// <summary>
        /// Detects PostgreSQL unique constraint violations (SQLSTATE 23505).
        /// </summary>
        private static bool IsUniqueConstraintViolation(DbUpdateException ex)
        {
            var message = ex.InnerException?.Message ?? ex.Message;
            return message.Contains("23505", StringComparison.Ordinal)
                || message.Contains("unique", StringComparison.OrdinalIgnoreCase)
                || message.Contains("IX_Colaboradores_Cpf", StringComparison.OrdinalIgnoreCase);
        }

        private static async Task EnsureUserMembershipsAsync(
            Context context,
            long userId,
            Entity.Cadastros.Empresa company,
            long actorUserId,
            CancellationToken cancellationToken)
        {
            var companyMembershipExists = await context.UsuariosEmpresas
                .AnyAsync(item => item.UsuarioId == userId && item.EmpresaId == company.Id, cancellationToken);
            if (!companyMembershipExists)
            {
                context.UsuariosEmpresas.Add(new UsuarioEmpresa
                {
                    UsuarioId = userId,
                    EmpresaId = company.Id,
                    UserId = actorUserId
                });
            }

            var tenantMembershipExists = await context.UsuariosTenants
                .AnyAsync(item => item.UsuarioId == userId && item.TenantId == company.TenantId, cancellationToken);
            if (!tenantMembershipExists)
            {
                context.UsuariosTenants.Add(new UsuarioTenant
                {
                    UsuarioId = userId,
                    TenantId = company.TenantId,
                    UserId = actorUserId
                });
            }
        }
    }
}
