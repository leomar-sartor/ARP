using ARP.Entity;
using ARP.Modules.Pessoa.Loaders;

namespace ARP.Modules.Pessoa
{
    [ExtendObjectType(typeof(Entity.Pessoa))]
    public class PessoaResolvers
    {
        public async Task<IReadOnlyList<Endereco>> GetEnderecos(
            [Parent] Entity.Pessoa pessoa,
            EnderecosByPessoaIdDataLoader dataLoader,
            CancellationToken ct)
        {
            if (pessoa == null)
                return Array.Empty<Endereco>();

            return await dataLoader.LoadAsync(pessoa.Id, ct)
                   ?? Array.Empty<Endereco>();
        }
    }
}
