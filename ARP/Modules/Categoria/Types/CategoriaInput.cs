namespace ARP.Modules.Categoria.Types
{
    [GraphQLDescription("Dados de entrada para categoria.")]
    public record CategoriaInput
    (
        [GraphQLDescription("Nome da categoria")]
        string Nome,

        [GraphQLDescription("Descrição")]
        string? Descricao
    );
}
