namespace ARP.Entity
{
    public class Setor : Base
    {
        public string Nome { get; set; } = default!;

        public string Descricao { get; set; } = default!;

        public ICollection<EmpresaSetor> EmpresaSetores { get; set; }
        = new List<EmpresaSetor>();

    }
}
