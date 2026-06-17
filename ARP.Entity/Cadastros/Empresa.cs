namespace ARP.Entity.Cadastros
{
    public class Empresa : Base
    {
        public string Cnpj { get; set; } = default!;
        public string? NomeFantasia { get; set; } = default!;

        public string? Descricao { get; set; } = default!;

        public ICollection<Setor> Setores { get; set; }
        = new List<Setor>();
        public ICollection<Colaborador> Colaboradores { get; set; }
        = new List<Colaborador>();

    }
}
