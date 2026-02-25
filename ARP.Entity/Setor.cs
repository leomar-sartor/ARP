namespace ARP.Entity
{
    public class Setor : Base
    {
        public string Nome { get; set; }

        public string Descricao { get; set; }

        //public ICollection<Empresa> Empresas { get; set; } = new List<Empresa>();

        public ICollection<EmpresaSetor> EmpresaSetores { get; set; }
        = new List<EmpresaSetor>();

    }
}
