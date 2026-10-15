using ARP.Entity;
using ARP.Entity.Cadastros;

namespace ARP.Entity.Pesquisas
{
    public class PesquisaEmpresa : Base
    {
        public long TenantId { get; set; }

        public long PesquisaId { get; set; }
        public Pesquisa Pesquisa { get; set; } = default!;

        public long EmpresaId { get; set; }
        public Empresa Empresa { get; set; } = default!;
    }
}