using HotChocolate.Execution.Configuration;

namespace ARP.Modules.Categoria
{
    public static class CategoriaModuleConfig
    {
        public static IRequestExecutorBuilder AddCategoriaQueriesAndMutations(
            this IRequestExecutorBuilder builder)
        {
            builder.AddTypeExtension<CategoriaQuery>();
            builder.AddTypeExtension<CategoriaMutation>();
            return builder;
        }
    }
}
