using ARP.Entity;

namespace ARP.Entity.Cadastros
{
    public class UsuarioEmpresa : Base
    {
        public long UsuarioId { get; set; }
        public Usuario Usuario { get; set; } = default!;

        public long EmpresaId { get; set; }
        public Empresa Empresa { get; set; } = default!;

        public bool Ativo { get; set; } = true;
    }
}