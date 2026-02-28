using HotChocolate.Execution.Configuration;

namespace ARP.Modules.Setor
{
    public static class SetorModuleConfig
    {
        public static IRequestExecutorBuilder AddSetorQueriesAndMutations(this IRequestExecutorBuilder builder)
        {
            builder.AddTypeExtension<SetorQuery>();
            builder.AddTypeExtension<SetorMutation>();
            return builder;
        }
    }
}
