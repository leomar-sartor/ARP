namespace ARP.Modules.Colaborador.Types
{
    [GraphQLDescription("Dados de entrada para colaborador")]
    public record ColaboradorInput(

        [GraphQLDescription("Identificador do cidadão.")]
        string Cpf,

        [GraphQLDescription("Nome do cidadão.")]
        string Nome,

        [GraphQLDescription("Email do cidadão.")]
        string Email,

        [GraphQLDescription("Identificador da empresa.")]
        long EmpresaId,

        [GraphQLDescription("Identificador do setor.")]
        long SetorId

        );
}
