namespace ARP.Modules.Empresa.Types
{
    [GraphQLDescription("Dados de entrada para empresa.")]
    public record EmpresaInput
    (
        [GraphQLDescription("Nome fantasia da empresa")]
        string RazaoSocial,

        [GraphQLDescription("Descrição")]
        string Descricao
    );
}
