namespace ARP.Entity.Pesquisas
{
    public class Categoria : Base
    {
        public string Nome { get; set; } = default!;

        public string? Descricao { get; set; }

        public bool Ativo { get; set; } = true;

        public ICollection<Questao> Questoes { get; set; } = new List<Questao>();
    }
}
