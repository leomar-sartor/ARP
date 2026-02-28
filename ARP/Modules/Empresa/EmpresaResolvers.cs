using ARP.Modules.Empresa.Loaders;

namespace ARP.Modules.Empresa
{
    [ExtendObjectType(typeof(Entity.Empresa))]
    public class EmpresaResolvers
    {
        public async Task<IReadOnlyList<Entity.Setor>> GetSetoresDaEmpresa(
            [Parent] Entity.Empresa empresa,
            SetoresByEmpresaIdDataLoader dataLoader,
            CancellationToken ct)
        {
            if (empresa is null)
                return Array.Empty<Entity.Setor>();

            return await dataLoader.LoadAsync(empresa.Id, ct)
                   ?? Array.Empty<Entity.Setor>();
        }
    }
}
