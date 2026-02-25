using ARP.Infra;
using ARP.Modules.Empresa;
using ARP.Modules.Pessoa.Types;

namespace ARP.Modules.Pessoa
{
    [ExtendObjectType("Mutation")]
    public class PessoaMutation
    {
        private readonly ILogger<PessoaMutation> _logger;

        public PessoaMutation(
            ILogger<PessoaMutation> logger
            )
        {
            _logger = logger;
        }

        [GraphQLDescription("Cadastrar Pessoa")]
        public async Task<Entity.Pessoa> CreatePessoa(
        PessoaInput input,
        [Service] Context context)
        {
            var pessoa = new Entity.Pessoa
            {
                Nome = input.Nome
            };

            context.Pessoas.Add(pessoa);
            await context.SaveChangesAsync();

            return pessoa;
        }

        [GraphQLDescription("Atualizar Empresa")]
        public async Task<Entity.Pessoa?> UpdatePessoa(
        long id,
        PessoaInput input,
        [Service] Context context)
        {
            var pessoa = await context.Pessoas.FindAsync(id);

            if (pessoa == null)
                return null;

            pessoa.Nome = input.Nome;

            await context.SaveChangesAsync();

            return pessoa;
        }

        [GraphQLDescription("Remover pessoa")]
        public async Task<bool> DeletePessoa(
        long id,
        [Service] Context context)
        {
            var pessoa = await context.Pessoas.FindAsync(id);

            if (pessoa == null)
                return false;


            pessoa.DeletedAt = DateTime.UtcNow;

            await context.SaveChangesAsync();

            return true;
        }
    }
}
