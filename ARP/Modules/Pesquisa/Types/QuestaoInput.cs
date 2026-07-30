using ARP.Entity.Enums;

namespace ARP.Modules.Pesquisa.Types
{
    [GraphQLDescription("Dados de entrada para criar questões")]
    public record QuestaoInput
    (
        [GraphQLDescription("Descritivo da questão.")]
        string Titulo,
        [GraphQLDescription("Texto ou com Opções")]
        TipoQuestao Tipo,
        [GraphQLDescription("Obrigatório a responder.")]
        bool Obrigatoria,
        [GraphQLDescription("Se pode escolher mais de uma opção")]
        bool MultiplasRespostas,
        [GraphQLDescription("Maximo de caracteres para uma resposta descritiva.")]
        int? MaximoDeCaracteres,
        [GraphQLDescription("Categoria da questão.")]
        long? CategoriaId,
        [GraphQLDescription("Opções disponíveis para a questão.")]
        IList<OpcaoInput>? Opcoes
    );
}
