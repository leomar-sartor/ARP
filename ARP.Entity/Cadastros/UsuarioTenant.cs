using ARP.Entity;

namespace ARP.Entity.Cadastros
{
    public class UsuarioTenant : Base
    {
        public long UsuarioId { get; set; }
        public Usuario Usuario { get; set; } = default!;

        public long TenantId { get; set; }
        public Tenant Tenant { get; set; } = default!;

        public bool Ativo { get; set; } = true;
    }
}