using ARP.Entity;

namespace ARP.Entity.Cadastros
{
    public class Persona : Base
    {
        public string Codigo { get; set; } = default!;
        public string Nome { get; set; } = default!;
        public bool Ativo { get; set; } = true;

        public ICollection<PersonaPermissao> Permissoes { get; set; } = new List<PersonaPermissao>();
        public ICollection<UsuarioPersona> Usuarios { get; set; } = new List<UsuarioPersona>();
    }
}