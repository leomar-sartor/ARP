namespace ARP.Entity
{
    public class Endereco : Base
    {
        public string Rua { get; set; } = default!;
        public string Cidade { get; set; } = default!;

        public long PessoaId { get; set; }

        public Pessoa Pessoa { get; set; } = default!;
    }
}
