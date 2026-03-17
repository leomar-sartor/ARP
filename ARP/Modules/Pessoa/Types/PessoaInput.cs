using ARP.Entity;

namespace ARP.Modules.Pessoa.Types
{
    [GraphQLDescription("Dados de entrada para pessoa")]
    public record PessoaInput(
        [GraphQLDescription("Nome do cidadão.")]
        string Nome,

        [GraphQLDescription("Enderecos")]
        IList<EnderecoInput> Enderecos
        );
}
