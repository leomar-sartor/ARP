using ARP.Entity.Cadastros;
using ARP.Entity.Enums;

namespace ARP.Entity.Pesquisas
{
    public class Pesquisa : Base
    {
        public string Nome { get; set; } = default!;
        public DateTime? DataInicial { get; set; }

        public DateTime? DataFinal { get; set; }

        public TipoPesquisa Tipo { get; set; } = TipoPesquisa.Flexivel;

        public long? TenantId { get; set; }
        public Tenant? Tenant { get; set; }

        public DateTime? PublicadaEm { get; set; }

        public ICollection<Questao> Questoes { get; set; }
            = new List<Questao>();

        public ICollection<PesquisaEmpresa> EmpresasVinculadas { get; set; }
            = new List<PesquisaEmpresa>();

        public ICollection<Convite> Convites { get; set; }
            = new List<Convite>();
    }
}
