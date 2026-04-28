//using HotChocolate;

namespace ARP.Entity
{
    public class Setor : Base
    {
        public string Nome { get; set; } = default!;

        public string Descricao { get; set; } = default!;

        //[GraphQLIgnore]
        public ICollection<EmpresaSetor> EmpresaSetores { get; set; }
        = new List<EmpresaSetor>();

        public ICollection<Colaborador> Colaboradores { get; set; }
        = new List<Colaborador>();
    }
}
