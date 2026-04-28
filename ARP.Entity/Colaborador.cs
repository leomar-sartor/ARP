namespace ARP.Entity
{
    public class Colaborador : Base
    {
        public string Cpf { get; set; } = default!;
        public string Nome { get; set; } = default!;

        public string Email { get; set; } = default!;

        public bool Ativo { get; set; } = true;


        public long SetorId { get; set; }
        public Setor Setor { get; set; } = default!;

        public long EmpresaId { get; set; }
        public Empresa Empresa { get; set; } = default!;
    }
}
