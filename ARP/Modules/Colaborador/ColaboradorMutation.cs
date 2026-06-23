using ARP.Infra;
using ARP.Modules.Colaborador.Types;
using ARP.Utils;
using Microsoft.EntityFrameworkCore;

namespace ARP.Modules.Colaborador
{
    [ExtendObjectType("Mutation")]
    public class ColaboradorMutation
    {
        private readonly ILogger<ColaboradorMutation> _logger;

        public ColaboradorMutation(
            ILogger<ColaboradorMutation> logger
            )
        {
            _logger = logger;
        }

        [GraphQLDescription("Cadastrar um novo colaborador")]
        public async Task<Entity.Cadastros.Colaborador> CreateColaborador(
        ColaboradorInput input,
        [Service] Context context,
        CancellationToken ct)
        {
            if(!CpfHelper.IsValidCpf(input.Cpf))
                throw new ArgumentException("CPF inválido");

            var empresa = await context.Empresas.FindAsync(new object[] { input.EmpresaId }, ct);

            if (empresa == null)
                throw new ArgumentException("Empresa não encontrada");

            var setor = await context.Setores.FindAsync(new object[] { input.SetorId }, ct);

            if (setor == null)
                throw new ArgumentException("Setor não encontrado");

            var entity = new Entity.Cadastros.Colaborador
            {
                Cpf = CpfHelper.OnlyDigits(input.Cpf),
                Nome = input.Nome,
                Email = input.Email,
                SetorId = input.SetorId,
                EmpresaId = input.EmpresaId
            };

            context.Colaboradores.Add(entity);

            await context.SaveChangesAsync(ct);

            return entity;
        }

        [GraphQLDescription("Atualizar uma colaborador existente")]
        public async Task<Entity.Cadastros.Colaborador?> UpdateColaborador(
            long Id,
            ColaboradorInput input,
            [Service] Context context,
            CancellationToken ct)
        {
            var entity = await context.Colaboradores 
                   .FirstOrDefaultAsync(p => p.Id == Id, ct);

            if (entity == null)
                return null; 

            entity.Cpf = input.Cpf;
            entity.Nome = input.Nome;
            entity.Email = input.Email;
            entity.SetorId = input.SetorId;
            entity.EmpresaId = input.EmpresaId;
            entity.UpdatedAt = DateTime.UtcNow;

            await context.SaveChangesAsync();

            return entity;
        }

        [GraphQLDescription("Remover um colaborador")]
        public async Task<bool> RemoveColaborador(
            long id,
            [Service] Context context,
            CancellationToken ct)
        {
            await using var transaction = await context.Database.BeginTransactionAsync(ct);

            try
            {
                var entity = await context.Colaboradores
                .FirstOrDefaultAsync(p => p.Id == id, ct);

                if (entity == null)
                    return false;

                var now = DateTime.UtcNow;
                entity.DeletedAt = now;

                await context.SaveChangesAsync();

                await transaction.CommitAsync(ct);
            }
            catch
            {
                await transaction.RollbackAsync(ct);
                throw;
            }

            return true;
        }
    }
}
