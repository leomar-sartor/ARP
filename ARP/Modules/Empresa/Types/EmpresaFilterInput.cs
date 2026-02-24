using HotChocolate.Data.Filters;

namespace ARP.Modules.Empresa.Types
{
    public class EmpresaFilterInput : FilterInputType<Entity.Empresa>
    {
        protected override void Configure(IFilterInputTypeDescriptor<Entity.Empresa> descriptor)
        {
            descriptor.BindFieldsImplicitly(); // Permite que o Hot Chocolate infira os campos
            // Você pode customizar aqui, por exemplo:
             //descriptor.Field(f => f.RazaoSocial).Type<StringType>();
             //descriptor.Field(f => f.CreatedAt).Type<DateTimeType>();
        }
    }
}
