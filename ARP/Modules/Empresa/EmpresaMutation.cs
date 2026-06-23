using ARP.Infra;
using ARP.Modules.Empresa.Types;
using ARP.Utils;

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
            [Service] Context context
            )
        {
            _logger.Log(LogLevel.Information, "Cadastrando Empresa");

            if (!CnpjHelper.IsValidCnpj(input.CNPJ))
                throw new ArgumentException("CNPJ inválido");

            var entity = new Entity.Cadastros.Empresa
            {
                Cnpj = CnpjHelper.OnlyLettersAndDigits(input.CNPJ),
                NomeFantasia = input.NomeFantasia,
                Descricao = input.Descricao,
                Ativo = true
            };

            context.Empresas.Add(entity);
            await context.SaveChangesAsync();

            return entity;
        }

        [GraphQLDescription("Atualizar uma empresa existente")]
        public async Task<Entity.Cadastros.Empresa?> UpdateEmpresa(
        long id,
        EmpresaInput input,
        [Service] Context context)
        {
            var entity = await context.Empresas.FindAsync(id);

            if (entity == null)
                return null;

            entity.NomeFantasia = input.NomeFantasia;
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

            var existAnySetor = context.Setores.Where(s => s.EmpresaId == id).Any();
            if (existAnySetor)
                throw new ArgumentException("Não é possível remover uma empresa que possui setores associados");

            var existAnyUser = context.Users.Where(u => u.EmpresaId == id).Any();
            if (existAnyUser)
                throw new ArgumentException("Não é possível remover uma empresa que possui usuários associados");

            entity.DeletedAt = DateTime.UtcNow; 

            await context.SaveChangesAsync();

            return true;
        }
    }
}