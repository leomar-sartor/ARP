namespace ARP.Entity.Pesquisas
{
    public class QuestaoResposta : Base
    {
        public string Token { get; set; } = default!;

        public string? TextoResposta { get; set; }

        public long QuestaoId { get; set; }
        public Questao Questao { get; set; } = default!;
        

        public long? QuestaoOpcaoId { get; set; }

        public QuestaoOpcao? QuestaoOpcao { get; set; }

    }
}
