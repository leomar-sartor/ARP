using ARP.Entity.Enums;
using ARP.Entity.Pesquisas;
using ARP.Modules.Pesquisa.Types;

namespace ARP.Modules.Pesquisa
{
    internal static class PesquisaPadronizadaDefinition
    {
        private static readonly (FrequenciaResposta Value, string Description)[] Options =
        [
            (FrequenciaResposta.Nunca, "Nunca"),
            (FrequenciaResposta.Raramente, "Raramente"),
            (FrequenciaResposta.AlgumasVezes, "Algumas Vezes"),
            (FrequenciaResposta.Frequente, "Frequente"),
            (FrequenciaResposta.MuitoFrequente, "Muito Frequente")
        ];

        public static Questao CreateQuestion(
            QuestaoPadronizadaInput input,
            int order,
            string categoryName)
        {
            var question = new Questao
            {
                Ordem = order,
                Titulo = input.Titulo.Trim(),
                Tipo = TipoQuestao.Opcao,
                Obrigatoria = input.Obrigatoria,
                MultiplasRespostas = false,
                CategoriaId = input.CategoriaId,
                CategoriaNomeSnapshot = categoryName
            };

            foreach (var option in Options)
            {
                question.Opcoes.Add(new QuestaoOpcao
                {
                    Ordem = (int)option.Value,
                    Valor = (int)option.Value,
                    Descricao = option.Description
                });
            }

            return question;
        }

        public static bool IsValidValue(int? value) =>
            value is >= (int)FrequenciaResposta.Nunca and <= (int)FrequenciaResposta.MuitoFrequente;

        public static bool HasFixedOptions(Questao question) =>
            question.Opcoes.Count == Options.Length
            && question.Opcoes.All(option => IsValidValue(option.Valor))
            && question.Opcoes.Select(option => option.Valor).Distinct().Count() == Options.Length
            && question.Opcoes.All(option =>
                Options.Any(expected => (int)expected.Value == option.Valor
                    && expected.Description == option.Descricao));
    }
}