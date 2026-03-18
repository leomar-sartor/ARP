namespace ARP.Entity
{
    public class Habilidade : Base
    {
        public string Nome { get; set; } = default!;

        public ICollection<PessoaHabilidade> PessoaHabilidades { get; set; }
            = new List<PessoaHabilidade>();
    }
}
