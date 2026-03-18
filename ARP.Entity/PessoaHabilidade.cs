namespace ARP.Entity
{
    public class PessoaHabilidade : Base
    {
        public long PessoaId { get; set; }
        public Pessoa Pessoa { get; set; } = default!;

        public long HabilidadeId { get; set; }
        public Habilidade Habilidade { get; set; } = default!;
    }
}
