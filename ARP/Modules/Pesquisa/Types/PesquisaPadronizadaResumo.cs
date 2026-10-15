using System.Linq.Expressions;

namespace ARP.Modules.Pesquisa.Types;

public sealed class PesquisaPadronizadaResumo
{
    public long Id { get; init; }
    public string Nome { get; init; } = string.Empty;
    public DateTime? PublicadaEm { get; init; }
    public DateTime? DataInicial { get; init; }
    public DateTime? DataFinal { get; init; }
    public IReadOnlyList<PerguntaResumo> Questoes { get; init; } = [];
    public IReadOnlyList<VinculoEmpresaResumo> EmpresasVinculadas { get; init; } = [];

    public static readonly Expression<Func<Entity.Pesquisas.Pesquisa, PesquisaPadronizadaResumo>> Projection = survey => new()
    {
        Id = survey.Id,
        Nome = survey.Nome,
        PublicadaEm = survey.PublicadaEm,
        DataInicial = survey.DataInicial,
        DataFinal = survey.DataFinal,
        Questoes = survey.Questoes.OrderBy(question => question.Ordem).Select(question => new PerguntaResumo(
            question.Id, question.Titulo, question.CategoriaId == null ? null : new CategoriaResumo(
                question.CategoriaId.Value, question.CategoriaNomeSnapshot ?? question.Categoria!.Nome))).ToArray(),
        EmpresasVinculadas = survey.EmpresasVinculadas.Select(link => new VinculoEmpresaResumo(
            new EmpresaResumo(link.Empresa.Id, link.Empresa.NomeFantasia))).ToArray()
    };
}

public sealed record PerguntaResumo(long Id, string Titulo, CategoriaResumo? Categoria);
public sealed record CategoriaResumo(long Id, string Nome);
public sealed record VinculoEmpresaResumo(EmpresaResumo Empresa);
public sealed record EmpresaResumo(long Id, string? NomeFantasia);
