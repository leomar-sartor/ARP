namespace ARP.Modules.Pessoa.Types
{
    [GraphQLDescription("Dados de entrada para habilidades")]
    public record HabilidadeInput(
         [GraphQLDescription("Nome da habilidade.")]
        string Nome
    );
}
