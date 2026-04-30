namespace ARP.Entity
{
    public class PesquisaRascunho : Base
    {
        public string Token { get; set; }

        public long PesquisaId { get; set; }

        public Pesquisa Pesquisa { get; set; } = default!;

        public long UltimaQuestaoRespondidaId { get; set; }
    }
}
