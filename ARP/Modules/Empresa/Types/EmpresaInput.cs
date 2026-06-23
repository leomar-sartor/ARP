namespace ARP.Modules.Empresa.Types
{
    [GraphQLDescription("Dados de entrada para empresa.")]
    public record EmpresaInput
    (
        [GraphQLDescription("CNPJ da empresa")]
        string CNPJ,

        [GraphQLDescription("Nome da empresa")]
        string? NomeFantasia,

        [GraphQLDescription("Descrição")]
        string? Descricao
    );
}
