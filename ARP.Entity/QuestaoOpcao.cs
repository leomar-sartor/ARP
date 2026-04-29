namespace ARP.Entity
{
    public class QuestaoOpcao : Base
    {
        public int Ordem { get; set; }
        public string Descricao { get; set; } = default!;
        public long QuestaoId { get; set; }
        public Questao Questao { get; set; } = default!;
    }
}