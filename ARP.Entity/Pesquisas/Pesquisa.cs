namespace ARP.Entity.Pesquisas
{
    public class Pesquisa : Base
    {
        public string Nome { get; set; } = default!;
        public DateTime? DataInicial { get; set; }

        public DateTime? DataFinal { get; set; }

        public ICollection<Questao> Questoes { get; set; }
            = new List<Questao>();

        public ICollection<Convite> Convites { get; set; }
            = new List<Convite>();
    }
}
