using ARP.Infra.Interfaces;

namespace ARP.Infra
{
    public class Repository : SqlUtil
    {
        public Repository(IConexao conexao) : base(conexao) { }
    }
}
