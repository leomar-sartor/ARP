using ARP.Modules.Pessoa;
using HotChocolate.Execution.Configuration;

namespace ARP.Modules.Empresa
{
    public static class PessoaModuleConfig
    {
        public static IRequestExecutorBuilder AddPessoaQueriesAndMutations(this IRequestExecutorBuilder builder)
        {
            builder.AddTypeExtension<PessoaQuery>();
            builder.AddTypeExtension<PessoaMutation>();
            return builder;
        }
    }
}
