using ARP.Entity;
using ARP.Infra;
using ARP.Infra.Interfaces;

namespace ARP.Repo
{
    public class EmpresaRepository : BaseRepository<Empresa>
    {
        private static readonly string select =
            @"SELECT 
                    E.Id,
                    E.RazaoSocial,
                    E.Descricao,
                    E.CreatedAt,
                    E.UpdatedAt,
                    E.DeletedAt
                 FROM 
                     {0} {1} 
                 ";

        public EmpresaRepository(IConexao conexao) : base(conexao, "Empresa", "E", select) { }

        protected override object GetParameters(Empresa dominio)
        {
            var parameters = new
            {
                dominio.Id,
                dominio.RazaoSocial,
                dominio.Descricao,
                dominio.CreatedAt,
                dominio.UpdatedAt
            };

            return parameters;
        }
    }
}
