using ARP.Entity;

namespace ARP.Entity.Cadastros
{
    public class Tenant : Base
    {
        public string Nome { get; set; } = default!;

        public bool Ativo { get; set; } = true;

        public ICollection<Empresa> Empresas { get; set; } = new List<Empresa>();
        public ICollection<UsuarioTenant> UsuariosVinculados { get; set; } = new List<UsuarioTenant>();
        public ICollection<UsuarioPersona> PersonasAtribuidas { get; set; } = new List<UsuarioPersona>();
    }
}