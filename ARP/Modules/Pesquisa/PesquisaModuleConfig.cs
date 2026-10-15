using HotChocolate.Execution.Configuration;

namespace ARP.Modules.Pesquisa
{
    public static class PesquisaModuleConfig
    {
        public static IRequestExecutorBuilder AddPesquisaQueriesAndMutations(this IRequestExecutorBuilder builder)
        {
            builder.AddTypeExtension<PesquisaQuery>();
            builder.AddTypeExtension<PesquisaMutation>();
            builder.AddTypeExtension<PesquisaPadronizadaQuery>();
            builder.AddTypeExtension<PesquisaPadronizadaMutation>();
            builder.AddTypeExtension<PesquisaOperationsQuery>();
            builder.AddTypeExtension<MinhasPesquisasQuery>();
            return builder;
        }
    }
}
