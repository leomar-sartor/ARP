namespace ARP.Modules.Pesquisa.Types
{
    [GraphQLDescription("Dados de entrada para criar pesquisa")]
    public record PesquisaInput
    (
        [GraphQLDescription("Nome da pesquisa.")]
        string Nome,
        [GraphQLDescription("Vigência Inicial.")]
        DateTime DataInicial,
        [GraphQLDescription("Vigência Final.")]
        DateTime DataFinal,
        [GraphQLDescription("Questões")]
        IList<QuestaoInput> Questoes
    );
}
