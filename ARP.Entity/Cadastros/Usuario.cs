using Microsoft.AspNetCore.Identity;

namespace ARP.Entity.Cadastros
{
    public class Usuario : IdentityUser<long>
    {
        public long? EmpresaId { get; set; } = default!;
        public string Cpf { get; set; } = default!;
        public bool Ativo { get; set; } = true;
    }
}
