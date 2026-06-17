using ARP.Modules.Empresa.Loaders;

namespace ARP.Modules.Empresa
{
    [ExtendObjectType(typeof(Entity.Cadastros.Empresa))]
    public class EmpresaResolvers
    {
        public async Task<IReadOnlyList<Entity.Cadastros.Setor>> GetSetoresDaEmpresa(
            [Parent] Entity.Cadastros.Empresa empresa,
            SetoresByEmpresaIdDataLoader dataLoader,
            CancellationToken ct)
        {
            if (empresa is null)
                return Array.Empty<Entity.Cadastros.Setor>();

            return await dataLoader.LoadAsync(empresa.Id, ct)
                   ?? Array.Empty<Entity.Cadastros.Setor>();
        }
    }
}
