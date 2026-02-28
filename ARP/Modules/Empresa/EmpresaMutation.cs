using ARP.Infra;
using ARP.Modules.Empresa.Types;

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
        public async Task<ARP.Entity.Empresa> CreateEmpresa(
            EmpresaInput input,
            [Service] Context context
            )
        {
            _logger.Log(LogLevel.Information, "Cadastrando Empresa");

            var entity = new Entity.Empresa
            {
                RazaoSocial = input.RazaoSocial,
                Descricao = input.Descricao
            };

            context.Empresas.Add(entity);
            await context.SaveChangesAsync();

            return entity;
        }

        [GraphQLDescription("Atualizar uma empresa existente")]
        public async Task<Entity.Empresa?> UpdateEmpresa(
        long id,
        EmpresaInput input,
        [Service] Context context)
        {
            var entity = await context.Empresas.FindAsync(id);

            if (entity == null)
                return null;

            entity.RazaoSocial = input.RazaoSocial;
            entity.Descricao = input.Descricao;

            await context.SaveChangesAsync();

            return entity;
        }

        [GraphQLDescription("Remover uma empresa")]
        public async Task<bool> RemoveEmpresa(
        long id,
        [Service] Context context)
        {
            var entity = await context.Empresas.FindAsync(id);

            if (entity == null)
                return false;

            entity.DeletedAt = DateTime.UtcNow;

            await context.SaveChangesAsync();

            return true;
        }
    }
}