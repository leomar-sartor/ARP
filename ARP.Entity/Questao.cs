using ARP.Entity.Enums;

namespace ARP.Entity
{
    public class Questao : Base
    {
        public string Titulo { get; set; } = default!;

        public TipoQuestao Tipo { get; set; }

        public bool Obrigatoria { get; set; } = false;

        public bool MultiplasRespostas { get; set; } = false;

        public int? MaximoDeCaracteres { get; set; } 

        public long PesquisaId { get; set; }
        public Pesquisa Pesquisa { get; set; } = default!;

        public ICollection<QuestaoOpcao> Opcoes { get; set; } = new List<QuestaoOpcao>();

        public ICollection<QuestaoResposta> Respostas { get; set; } = new List<QuestaoResposta>();
    }
}
