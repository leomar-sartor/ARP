namespace ARP.Entity
{
    public class Convite : Base
    {
        public string Token { get; set; } = default!;
        
        public DateTime EnviadoEm { get; set; }

        public DateTime IniciadoEm { get; set; }

        public DateTime ConcluidoEm { get; set; }

        public long PesquisaId { get; set; }

        public Pesquisa Pesquisa { get; set; } = default!;

    }
}
