using ARP.Entity;
using ARP.Infra;
using ARP.Infra.Interfaces;

namespace ARP.Repo
{
    public class SetorRepository : BaseRepository<Setor>
    {
        private static readonly string select =
            @"SELECT 
                    S.Id,
                    S.Nome,
                    S.Descricao,
                    S.CreatedAt,
                    S.UpdatedAt,
                    S.DeletedAt
                 FROM 
                     {0} {1} 
                 ";

        public SetorRepository(IConexao conexao) : base(conexao, "Setor", "S", select) { }

        protected override object GetParameters(Setor dominio)
        {
            var parameters = new
            {
                dominio.Id,
                dominio.Nome,
                dominio.Descricao,
                dominio.CreatedAt,
                dominio.UpdatedAt
            };

            return parameters;
        }
    }
}
