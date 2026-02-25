namespace ARP.Entity
{
    public class EmpresaSetor : Base
    {
        public long EmpresaId { get; set; }
        public Empresa Empresa { get; set; } = default!;

        public long SetorId { get; set; }
        public Setor Setor { get; set; } = default!;
    }
}
