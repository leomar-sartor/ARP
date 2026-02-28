using ARP.Infra;
using ARP.Modules.Setor.Types;

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
        public async Task<Entity.Setor> CreateSetor(
            long EmpresaId,
            SetorInput input,
            [Service] Context context)
        {
            var entity = new Entity.Setor
            {
                Nome = input.Nome,
                Descricao = input.Descricao
            };

            context.Setores.Add(entity);
            await context.SaveChangesAsync();

            var relation = new Entity.EmpresaSetor
            {
                EmpresaId = EmpresaId,
                SetorId = entity.Id
            };

            context.EmpresaSetores.Add(relation);
            await context.SaveChangesAsync();

            return entity;
        }

        [GraphQLDescription("Atualizar um setor existente")]
        public async Task<Entity.Setor?> UpdateSetor(
        long id,
        SetorInput input,
        [Service] Context context)
        {
            var entity = await context.Setores.FindAsync(id);

            if (entity == null)
                return null;

            entity.Nome = input.Nome;
            entity.Descricao = input.Descricao;

            await context.SaveChangesAsync();

            return entity;
        }

        [GraphQLDescription("Remover uma setor")]
        public async Task<bool> RemoveSetor(
        long id,
        [Service] Context context)
        {
            var entity = await context.Setores.FindAsync(id);

            if (entity == null)
                return false;

            entity.DeletedAt = DateTime.UtcNow;

            var relations = context.EmpresaSetores.Where(r => r.SetorId == id);

            if (relations != null)
                relations.ToList().ForEach(r => r.DeletedAt = DateTime.UtcNow);

            await context.SaveChangesAsync();

            return true;
        }
    }
}
