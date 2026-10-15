using ARP.Infra;
using ARP.Entity.Cadastros;
using ARP.Modules.Auth;
using ARP.Modules.Empresa.Types;
using ARP.Utils;
using Microsoft.EntityFrameworkCore;

namespace ARP.Modules.Empresa
{
    [ExtendObjectType("Mutation")]
    public class EmpresaMutation
    {
        private readonly ILogger<EmpresaMutation> _logger;

        public EmpresaMutation(
            ILogger<EmpresaMutation> logger
            )
        {
            _logger = logger;
        }

        [GraphQLDescription("Cadastra uma nova empresa")]
        public async Task<Entity.Cadastros.Empresa> CreateEmpresa(
            EmpresaInput input,
            [Service] Context context,
            [Service] TenantContextResolver tenantContextResolver,
            [Service] PermissionAuthorizationService permissionAuthorizationService,
            CancellationToken cancellationToken
            )
        {
            var activeContext = await tenantContextResolver.ResolveAsync(context, cancellationToken);
            await permissionAuthorizationService.RequireAsync(
                context, activeContext, PermissionCodes.CompanyCreate, cancellationToken);
            var tenantId = activeContext.TenantId
                ?? throw new GraphQLException("Selecione um tenant para cadastrar a empresa.");

            _logger.Log(LogLevel.Information, "Cadastrando Empresa");

            if (!CnpjHelper.IsValidCnpj(input.CNPJ))
                throw new ArgumentException("CNPJ inválido");

            var entity = new Entity.Cadastros.Empresa
            {
                Cnpj = CnpjHelper.OnlyLettersAndDigits(input.CNPJ),
                NomeFantasia = input.NomeFantasia,
                Descricao = input.Descricao,
                TenantId = tenantId,
                Ativo = true
            };

            context.Empresas.Add(entity);

            try
            {
                await context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                throw new ArgumentException($"Erro: {ex.InnerException}");
            }

            return entity;
        }

        [GraphQLDescription("Atualizar uma empresa existente")]
        public async Task<Entity.Cadastros.Empresa?> UpdateEmpresa(
        long id,
        EmpresaInput input,
        [Service] Context context,
        [Service] TenantContextResolver tenantContextResolver,
        [Service] PermissionAuthorizationService permissionAuthorizationService,
        CancellationToken cancellationToken)
        {
            var activeContext = await tenantContextResolver.ResolveAsync(context, cancellationToken);
            await permissionAuthorizationService.RequireAsync(
                context, activeContext, PermissionCodes.CompanyEdit, cancellationToken);
            var entity = await context.Empresas
                .FirstOrDefaultAsync(e => e.Id == id, cancellationToken);

            if (entity == null)
                return null;

            await tenantContextResolver.EnsureCanAccessCompanyAsync(
                context, activeContext, entity.Id, cancellationToken);

            if (!CnpjHelper.IsValidCnpj(input.CNPJ))
                throw new ArgumentException("CNPJ inválido");

            entity.Cnpj = CnpjHelper.OnlyLettersAndDigits(input.CNPJ);
            entity.NomeFantasia = input.NomeFantasia;
            entity.Descricao = input.Descricao;

            await context.SaveChangesAsync();

            return entity;
        }

        [GraphQLDescription("Remover uma empresa")]
        public async Task<bool> RemoveEmpresa(
        long id,
        [Service] Context context,
        [Service] TenantContextResolver tenantContextResolver,
        [Service] PermissionAuthorizationService permissionAuthorizationService,
        CancellationToken cancellationToken)
        {
            var activeContext = await tenantContextResolver.ResolveAsync(context, cancellationToken);
            await permissionAuthorizationService.RequireAsync(
                context, activeContext, PermissionCodes.CompanyManage, cancellationToken);
            var entity = await context.Empresas
                .FirstOrDefaultAsync(e => e.Id == id, cancellationToken);

            if (entity == null)
                return false;

            await tenantContextResolver.EnsureCanAccessCompanyAsync(
                context, activeContext, entity.Id, cancellationToken);

            var existAnySetor = context.Setores.Where(s => s.EmpresaId == id).Any();
            if (existAnySetor)
                throw new ArgumentException("Não é possível remover uma empresa que possui setores associados");

            var existAnyUser = context.Users.Where(u => u.EmpresaId == id).Any();
            var hasMemberships = await context.UsuariosEmpresas
                .AnyAsync(u => u.EmpresaId == id && u.Ativo, cancellationToken);
            if (existAnyUser || hasMemberships)
                throw new ArgumentException("Não é possível remover uma empresa que possui usuários associados");

            entity.DeletedAt = DateTime.UtcNow;

            await context.SaveChangesAsync();

            return true;
        }
    }
}