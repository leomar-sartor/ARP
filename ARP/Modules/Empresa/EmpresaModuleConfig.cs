using HotChocolate.Execution.Configuration;

namespace ARP.Modules.Empresa
{
    public static class EmpresaModuleConfig
    {
        public static IRequestExecutorBuilder AddEmpresaQueriesAndMutations(this IRequestExecutorBuilder builder)
        {
            builder.AddTypeExtension<EmpresaQuery>();
            builder.AddTypeExtension<EmpresaMutation>();
            return builder;
        }
    }
}
