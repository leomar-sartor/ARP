using HotChocolate.Data.Sorting;

namespace ARP.Modules.Empresa.Types
{
    public class EmpresaSortInput : SortInputType<Entity.Empresa>
    {
        protected override void Configure(ISortInputTypeDescriptor<Entity.Empresa> descriptor)
        {
            descriptor.BindFieldsImplicitly(); // Permite que o Hot Chocolate infira os campos
            // Você pode customizar aqui, por exemplo:
            // descriptor.Field(f => f.RazaoSocial).Type<StringType>().Name("razaoSocial");
        }
    }
}
