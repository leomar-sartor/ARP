namespace ARP.Entity.Cadastros
{
    public class PersonaPermissao
    {
        public long PersonaId { get; set; }
        public Persona Persona { get; set; } = default!;

        public long PermissaoId { get; set; }
        public Permissao Permissao { get; set; } = default!;
    }
}