using ARP.Entity;

namespace ARP.Entity.Cadastros
{
    public class Permissao : Base
    {
        public string Codigo { get; set; } = default!;
        public string Descricao { get; set; } = default!;
        public bool Ativo { get; set; } = true;

        public ICollection<PersonaPermissao> Personas { get; set; } = new List<PersonaPermissao>();
    }
}