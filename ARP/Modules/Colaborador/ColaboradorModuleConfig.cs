using HotChocolate.Execution.Configuration;

namespace ARP.Modules.Colaborador
{
    public static class ColaboradorModuleConfig
    {
        public static IRequestExecutorBuilder AddColaboradorQueriesAndMutations(this IRequestExecutorBuilder builder)
        {
            builder.AddTypeExtension<ColaboradorQuery>();
            builder.AddTypeExtension<ColaboradorMutation>();
            return builder;
        }
    }
}
