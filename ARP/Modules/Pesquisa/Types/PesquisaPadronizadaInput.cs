namespace ARP.Modules.Pesquisa.Types
{
    public sealed record PesquisaPadronizadaInput(
        string Nome,
        DateTime? DataInicial,
        DateTime? DataFinal,
        long[] EmpresaIds,
        IList<QuestaoPadronizadaInput> Questoes);

    public sealed record QuestaoPadronizadaInput(
        string Titulo,
        bool Obrigatoria,
        long CategoriaId);
}