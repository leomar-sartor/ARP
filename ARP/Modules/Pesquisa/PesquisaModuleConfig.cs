using HotChocolate.Execution.Configuration;

namespace ARP.Modules.Pesquisa
{
    public static class PesquisaModuleConfig
    {
        public static IRequestExecutorBuilder AddPesquisaQueriesAndMutations(this IRequestExecutorBuilder builder)
        {
            builder.AddTypeExtension<PesquisaQuery>();
            builder.AddTypeExtension<PesquisaMutation>();
            return builder;
        }
    }
}
