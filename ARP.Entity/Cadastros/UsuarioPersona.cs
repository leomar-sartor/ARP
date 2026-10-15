using ARP.Entity;

namespace ARP.Entity.Cadastros
{
    public class UsuarioPersona : Base
    {
        public long UsuarioId { get; set; }
        public Usuario Usuario { get; set; } = default!;

        public long PersonaId { get; set; }
        public Persona Persona { get; set; } = default!;

        public long TenantId { get; set; }
        public Tenant Tenant { get; set; } = default!;

        public long? EmpresaId { get; set; }
        public Empresa? Empresa { get; set; }

        public bool Ativo { get; set; } = true;
    }
}