using ARP.Entity.Cadastros;
using ARP.Entity.Enums;

namespace ARP.Entity.Pesquisas
{
    public class Convite : Base
    {
        public string Token { get; set; } = Guid.NewGuid().ToString("N");

        public string Hash { get; set; } = default!;

        public long ColaboradorId { get; set; }
        public Colaborador Colaborador { get; set; } = default!;

        public DateTime EnviadoEm { get; set; }

        //Deixar nulavel
        public DateTime IniciadoEm { get; set; }

        //Deixar nulavel
        public DateTime ConcluidoEm { get; set; }

        public long PesquisaId { get; set; }

        public Pesquisa Pesquisa { get; set; } = default!;

        public Status Status { get; set; } = Status.Pendente;

    }
}
