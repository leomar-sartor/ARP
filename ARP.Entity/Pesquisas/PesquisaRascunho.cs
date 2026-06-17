namespace ARP.Entity.Pesquisas
{
    public class PesquisaRascunho : Base
    {
        public string Token { get; set; }

        public long PesquisaId { get; set; }

        public Pesquisa Pesquisa { get; set; } = default!;

        public long UltimaQuestaoRespondidaId { get; set; }

        // NOVO: guarda o JSON das respostas parciais
        public string? RespostasParciais { get; set; } // JSON serializado

        public DateTime UltimaAtualizacao { get; set; } = DateTime.UtcNow;
    }
}
