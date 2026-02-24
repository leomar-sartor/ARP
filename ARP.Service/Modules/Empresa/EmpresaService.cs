using ARP.Infra.Interfaces;
using ARP.Entity;
using ARP.Repo;
using Dapper;

namespace ARP.Service.Modules.Empresa
{
    public class EmpresaService
    {
        public IConexao _conexao;

        private readonly EmpresaRepository _rEmpresa;

        public EmpresaService(IConexao conexao)
        {
            _conexao = conexao;

            _rEmpresa = new EmpresaRepository(_conexao);
        }

        public async Task<List<ARP.Entity.Empresa>> BuscarTodosFilter()
        {
            var results = await _rEmpresa.BuscarTodosAsync();

            return results.ToList();
        }

        public async Task<ARP.Entity.Empresa> Create(ARP.Entity.Empresa empresa)
        {
            empresa.Id = await _rEmpresa.SalvarAsync(empresa);

            return empresa;
        }

    }
}
