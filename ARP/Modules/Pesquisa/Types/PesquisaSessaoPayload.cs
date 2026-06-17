// ARP/Modules/Pesquisa/Types/PesquisaSessaoPayload.cs
namespace ARP.Modules.Pesquisa.Types
{
    [GraphQLDescription("Dados da sessão de uma pesquisa em andamento")]
    public record PesquisaSessaoPayload(
        [GraphQLDescription("Pesquisa com questões e opções")]
        Entity.Pesquisas.Pesquisa Pesquisa,

        [GraphQLDescription("ID da última questão respondida (null se não iniciou)")]
        long? UltimaQuestaoRespondidaId,

        [GraphQLDescription("Respostas parciais em JSON (null se não iniciou)")]
        string? RespostasParciais
    );
}