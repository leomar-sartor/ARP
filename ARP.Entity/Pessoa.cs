namespace ARP.Entity
{
    public class Pessoa : Base
    {
        public string Nome { get; set; } = default!;
        public ICollection<Endereco> Enderecos { get; set; }
            = new List<Endereco>();
    }
}
