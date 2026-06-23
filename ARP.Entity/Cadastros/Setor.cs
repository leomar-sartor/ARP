namespace ARP.Entity.Cadastros
{
    public class Setor : Base
    {
        public string Nome { get; set; } = default!;

        public string? Descricao { get; set; } = default!;

        public bool Ativo { get; set; } = true;
        public long EmpresaId { get; set; }
        public Empresa Empresa { get; set; } = default!;
        public ICollection<Colaborador> Colaboradores { get; set; }
        = new List<Colaborador>();
    }
}
