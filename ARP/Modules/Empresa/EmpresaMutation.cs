using ARP.Infra.Interfaces;
using ARP.Service.Modules.Empresa;
using HotChocolate.Authorization;

namespace ARP.Modules.Empresa
{
    //[Authorize]
    [ExtendObjectType("Mutation")]
    public class EmpresaMutation
    {
        private readonly IConexao _conexao;
        private readonly ILogger<EmpresaMutation> _logger;

        private readonly EmpresaService _empresaService;

        public EmpresaMutation(
            IConexao conexao,
            ILogger<EmpresaMutation> logger
            )
        {
            _conexao = conexao;
            _logger = logger;

            _empresaService = new EmpresaService(_conexao);
        }


        [GraphQLDescription("Cadastrar Empresa")]
        public async Task<ARP.Entity.Empresa> Cadastrar(string nome)
        {
            _logger.Log(LogLevel.Information, "Cadastrando Empresa");
            _logger.Log(LogLevel.Warning, "Cadastrando Empresa");
            _logger.Log(LogLevel.Error, "Cadastrando Empresa");
            _logger.Log(LogLevel.Critical, "Cadastrando Empresa");

            var empresa = new ARP.Entity.Empresa
            {
                RazaoSocial = nome,
                Descricao = nome
            };

            await _empresaService.Create(empresa);

            return empresa;
        }

    }
}