namespace ARP.Modules.Pessoa.Types
{
    [GraphQLDescription("Dados de entrada para endereço")]
    public record EnderecoInput(
        [GraphQLDescription("Rua.")]
        string Rua,
        [GraphQLDescription("Nome da Cidade.")]
        string Cidade
    );
}
