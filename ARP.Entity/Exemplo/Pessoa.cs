namespace ARP.Entity.Exemplo
{
    public class Pessoa : Base
    {
        public string Nome { get; set; } = default!;
        public ICollection<Endereco> Enderecos { get; set; }
            = new List<Endereco>();

        public ICollection<PessoaHabilidade> PessoaHabilidades { get; set; }
        = new List<PessoaHabilidade>();
    }
}
