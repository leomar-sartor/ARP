using ARP.Entity;

namespace ARP.Entity.Cadastros
{
    public class ColaboradorEmpresa : Base
    {
        public long ColaboradorId { get; set; }
        public Colaborador Colaborador { get; set; } = default!;

        public long EmpresaId { get; set; }
        public Empresa Empresa { get; set; } = default!;

        public long SetorId { get; set; }
        public Setor Setor { get; set; } = default!;

        public bool Ativo { get; set; } = true;
    }
}