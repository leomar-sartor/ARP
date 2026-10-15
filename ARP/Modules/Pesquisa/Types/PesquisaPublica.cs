using ARP.Entity.Enums;

namespace ARP.Modules.Pesquisa.Types;

// A sessão de resposta expõe apenas a estrutura, sem navegações de participação ou identidade.
public sealed record PesquisaPublica(long Id, string Nome, DateTime? DataInicial, DateTime? DataFinal,
    IReadOnlyList<QuestaoPublica> Questoes)
{
    public static PesquisaPublica From(Entity.Pesquisas.Pesquisa survey) => new(
        survey.Id, survey.Nome, survey.DataInicial, survey.DataFinal,
        survey.Questoes.OrderBy(question => question.Ordem).Select(question => new QuestaoPublica(
            question.Id, question.Ordem, question.Titulo, question.Tipo, question.Obrigatoria,
            question.MultiplasRespostas, question.MaximoDeCaracteres, question.CategoriaId,
            question.CategoriaNomeSnapshot,
            question.Opcoes.OrderBy(option => option.Ordem).Select(option => new OpcaoPublica(
                option.Id, option.Ordem, option.Valor, option.Descricao)).ToArray())).ToArray());
}

public sealed record QuestaoPublica(long Id, int Ordem, string Titulo, TipoQuestao Tipo, bool Obrigatoria,
    bool MultiplasRespostas, int? MaximoDeCaracteres, long? CategoriaId, string? CategoriaNomeSnapshot,
    IReadOnlyList<OpcaoPublica> Opcoes);

public sealed record OpcaoPublica(long Id, int Ordem, int? Valor, string Descricao);
