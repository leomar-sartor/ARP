namespace ARP.Modules.Setor.Types
{
    [GraphQLDescription("Dados de entrada para setor.")]
    public record SetorInput
    (
        [GraphQLDescription("Nome do setor")]
        string Nome,

        [GraphQLDescription("Descrição")]
        string Descricao
    );
}
