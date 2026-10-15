using Microsoft.AspNetCore.Identity;

namespace ARP.Entity.Cadastros
{
    public class Usuario : IdentityUser<long>
    {
        public long? EmpresaId { get; set; } = default!;
        public string Cpf { get; set; } = default!;
        public bool Ativo { get; set; } = true;

        public ICollection<UsuarioEmpresa> EmpresasVinculadas { get; set; } = new List<UsuarioEmpresa>();
        public ICollection<UsuarioTenant> TenantsVinculados { get; set; } = new List<UsuarioTenant>();
        public ICollection<UsuarioPersona> PersonasAtribuidas { get; set; } = new List<UsuarioPersona>();
    }
}
