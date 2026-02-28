namespace ARP.Modules.Empresa
{

    [ExtendObjectType("Mutation")]
    public class EmpresaMutation
    {
        private readonly ILogger<EmpresaMutation> _logger;

        public EmpresaMutation(
            ILogger<EmpresaMutation> logger
            )
        {
            _logger = logger;
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

            return empresa;
        }
    }
}