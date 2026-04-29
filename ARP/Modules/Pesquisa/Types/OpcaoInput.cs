namespace ARP.Modules.Pesquisa.Types
{
    [GraphQLDescription("Dados de entrada para criar opções de questão")]
    public record OpcaoInput
    (
        [GraphQLDescription("Ordem da opção.")]
        int Ordem,
        [GraphQLDescription("Descrição da opção.")]
        string Descricao
    );
}
