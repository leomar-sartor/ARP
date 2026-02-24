using ARP.Infra.Interfaces;
using ARP.Service.Modules.Empresa;

namespace ARP.Modules.Empresa
{
    [ExtendObjectType("Query")]
    public class EmpresaQuery
    {
        private readonly IConexao _conexao;
        private readonly ILogger<EmpresaQuery> _logger;

        private readonly EmpresaService _empresaService;

        public EmpresaQuery(
            IConexao conexao,
            ILogger<EmpresaQuery> logger
            )
        {
            _conexao = conexao;
            _logger = logger;

            _empresaService = new EmpresaService(_conexao);
        }

        [GraphQLDescription("Buscar Empresas")]
        public async Task<List<ARP.Entity.Empresa>> Buscar(string valor)
        {
            _logger.Log(LogLevel.Information, "Buscando Empresas");

            var results = await _empresaService.BuscarTodosFilter();

            return results;
        }
    }
}
