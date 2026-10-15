namespace ARP.Entity.Cadastros
{
    public class Empresa : Base
    {
        public string Cnpj { get; set; } = default!;
        public string? NomeFantasia { get; set; } = default!;
        public string? Descricao { get; set; } = default!;

        public long TenantId { get; set; }
        public Tenant Tenant { get; set; } = default!;

        public bool Ativo { get; set; } = true;
        public ICollection<Setor> Setores { get; set; }
        = new List<Setor>();
        public ICollection<Colaborador> Colaboradores { get; set; }
        = new List<Colaborador>();
        public ICollection<ColaboradorEmpresa> ColaboradoresVinculados { get; set; }
        = new List<ColaboradorEmpresa>();
        public ICollection<UsuarioEmpresa> UsuariosVinculados { get; set; }
        = new List<UsuarioEmpresa>();
        public ICollection<UsuarioPersona> UsuariosPersonas { get; set; }
        = new List<UsuarioPersona>();

    }
}
