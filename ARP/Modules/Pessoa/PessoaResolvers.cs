using ARP.Entity.Exemplo;
using ARP.Modules.Pessoa.Loaders;

namespace ARP.Modules.Pessoa
{
    [ExtendObjectType(typeof(Entity.Exemplo.Pessoa))]
    public class PessoaResolvers
    {
        public async Task<IReadOnlyList<Endereco>> GetEnderecos(
            [Parent] Entity.Exemplo.Pessoa pessoa,
            EnderecosByPessoaIdDataLoader dataLoader,
            CancellationToken ct)
        {
            if (pessoa == null)
                return Array.Empty<Endereco>();

            return await dataLoader.LoadAsync(pessoa.Id, ct)
                   ?? Array.Empty<Endereco>();
        }

        public async Task<IReadOnlyList<Habilidade>> GetHabilidades(
               [Parent] Entity.Exemplo.Pessoa pessoa,
               HabilidadesByPessoaIdDataLoader dataLoader,
               CancellationToken ct)
        {
            return await dataLoader.LoadAsync(pessoa.Id, ct)
                   ?? Array.Empty<Habilidade>();
        }
    }
}
