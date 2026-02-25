namespace ARP.Entity
{
    public class Empresa : Base
    {
        public string RazaoSocial { get; set; } = default!;

        public string Descricao { get; set; } = default!;

        public ICollection<Setor> Setores { get; set; } = new List<Setor>();

    }
}
