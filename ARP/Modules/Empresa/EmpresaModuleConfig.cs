using ARP.Modules.Empresa.Types;
using ARP.Service.Modules.Empresa;
using HotChocolate.Execution.Configuration;

namespace ARP.Modules.Empresa
{
    public static class EmpresaModuleConfig
    {
        public static IRequestExecutorBuilder AddEmpresaQueriesAndMutations(this IRequestExecutorBuilder builder)
        {
            builder.AddType<EmpresaFilterInput>(); 
            builder.AddType<EmpresaSortInput>();
            builder.AddTypeExtension<EmpresaQuery>();
            builder.AddTypeExtension<EmpresaMutation>();
            return builder;
        }
    }
}
