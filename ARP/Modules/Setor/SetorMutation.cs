using ARP.Infra;
using ARP.Entity.Cadastros;
using ARP.Modules.Auth;
using ARP.Modules.Setor.Types;
using Microsoft.EntityFrameworkCore;

namespace ARP.Modules.Setor
{
    [ExtendObjectType("Mutation")]
    public class SetorMutation
    {
        private readonly ILogger<SetorMutation> _logger;

        public SetorMutation(
            ILogger<SetorMutation> logger
            )
        {
            _logger = logger;
        }

        [GraphQLDescription("Cadastrar um novo Setor")]
        public async Task<Entity.Cadastros.Setor> CreateSetor(
            long EmpresaId,
            SetorInput input,
            [Service] Context context,
            [Service] TenantContextResolver tenantContextResolver,
            [Service] PermissionAuthorizationService permissionAuthorizationService,
            CancellationToken cancellationToken)
        {
            var activeContext = await tenantContextResolver.ResolveAsync(context, cancellationToken);
            await permissionAuthorizationService.RequireAsync(
                context, activeContext, PermissionCodes.SectorManage, cancellationToken);
            await tenantContextResolver.EnsureCanAccessCompanyAsync(
                context, activeContext, EmpresaId, cancellationToken);

            var empresa = await context.Empresas
                .FirstOrDefaultAsync(e => e.Id == EmpresaId, cancellationToken);

            if (empresa == null)
                throw new ArgumentException("Empresa não encontrada");

            var entity = new Entity.Cadastros.Setor
            {
                EmpresaId = EmpresaId,
                Nome = input.Nome,
                Descricao = input.Descricao,
                Ativo = true
            };

            context.Setores.Add(entity);

            await context.SaveChangesAsync(cancellationToken);

            return entity;
        }

        [GraphQLDescription("Atualizar um setor existente")]
        public async Task<Entity.Cadastros.Setor?> UpdateSetor(
        long id,
        SetorInput input,
        [Service] Context context,
        [Service] TenantContextResolver tenantContextResolver,
        [Service] PermissionAuthorizationService permissionAuthorizationService,
        CancellationToken cancellationToken)
        {
            var activeContext = await tenantContextResolver.ResolveAsync(context, cancellationToken);
            await permissionAuthorizationService.RequireAsync(
                context, activeContext, PermissionCodes.SectorManage, cancellationToken);
            var entity = await context.Setores
                .FirstOrDefaultAsync(s => s.Id == id, cancellationToken);

            if (entity == null)
                return null;

            await tenantContextResolver.EnsureCanAccessCompanyAsync(
                context, activeContext, entity.EmpresaId, cancellationToken);

            entity.Nome = input.Nome;
            entity.Descricao = input.Descricao;

            await context.SaveChangesAsync(cancellationToken);

            return entity;
        }

        [GraphQLDescription("Remover uma setor")]
        public async Task<bool> RemoveSetor(
        long id,
        [Service] Context context,
        [Service] TenantContextResolver tenantContextResolver,
        [Service] PermissionAuthorizationService permissionAuthorizationService,
        CancellationToken cancellationToken)
        {
            var activeContext = await tenantContextResolver.ResolveAsync(context, cancellationToken);
            await permissionAuthorizationService.RequireAsync(
                context, activeContext, PermissionCodes.SectorManage, cancellationToken);
            var entity = await context.Setores
                .FirstOrDefaultAsync(s => s.Id == id, cancellationToken);

            if (entity == null)
                return false;

            await tenantContextResolver.EnsureCanAccessCompanyAsync(
                context, activeContext, entity.EmpresaId, cancellationToken);

            var existAnyColaborador = await context.ColaboradorEmpresas
                .AnyAsync(c => c.SetorId == id && c.Ativo, cancellationToken);
            if (existAnyColaborador)
                throw new ArgumentException("Não é possível remover um setor que possui colaboradores associados");

            entity.DeletedAt = DateTime.UtcNow;

            await context.SaveChangesAsync(cancellationToken);

            return true;
        }
    }
}

