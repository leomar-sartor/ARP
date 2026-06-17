using HotChocolate.Execution.Configuration;

namespace ARP.Modules.Empresa
{
    public static class EmpresaModuleConfig
    {
        public static IRequestExecutorBuilder AddEmpresaQueriesAndMutations(this IRequestExecutorBuilder builder)
        {
            builder.AddType<Entity.Cadastros.Empresa>();
            builder.AddTypeExtension<EmpresaQuery>();
            builder.AddTypeExtension<EmpresaMutation>();
            builder.AddTypeExtension<EmpresaResolvers>();
            return builder;
        }
    }
}
