using ARP.Entity;
using ARP.Modules.Empresa.Loaders;

namespace ARP.Modules.Empresa
{
    [ExtendObjectType(typeof(Entity.Empresa))]
    public class EmpresaResolvers
    {
        public async Task<IReadOnlyList<Setor>> GetSetoresDaEmpresa(
            [Parent] Entity.Empresa empresa,
            SetoresByEmpresaIdDataLoader dataLoader,
            CancellationToken ct)
        {
            if (empresa is null)
                return Array.Empty<Setor>();

            return await dataLoader.LoadAsync(empresa.Id, ct)
                   ?? Array.Empty<Setor>();
        }
    }
}
