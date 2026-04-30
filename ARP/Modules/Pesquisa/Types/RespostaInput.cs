namespace ARP.Modules.Pesquisa.Types
{
    [GraphQLDescription("Dados de entrada para criar respostas")]
    public record RespostaInput
    (
        [GraphQLDescription("Identificador")]
        string Token,

        [GraphQLDescription("Identificador da questão.")]
        long QuestaoId,

        [GraphQLDescription("Resposta escrita para a questão.")]
        string? TextoResposta,

        [GraphQLDescription("Resposta selecionadas para as questão (uma ou mais).")]
        long[]? QuestaoOpcaoIds
    );
}
