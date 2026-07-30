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
            if (!CpfHelper.IsValidCpf(input.Cpf))
                throw new ArgumentException("CPF inválido");

            var cpf = CpfHelper.OnlyDigits(input.Cpf);

            var cpfAlreadyExists = await context.Colaboradores
                .AnyAsync(c => c.Cpf == cpf, ct);

            if (cpfAlreadyExists)
                throw new ArgumentException("Já existe um cadastro com este CPF.");

            var empresa = await context.Empresas.FindAsync(new object[] { input.EmpresaId }, ct);

            if (empresa == null)
                throw new ArgumentException("Empresa não encontrada");

            var setor = await context.Setores.FindAsync(new object[] { input.SetorId }, ct);

            if (setor == null)
                throw new ArgumentException("Setor não encontrado");

            var entity = new Entity.Cadastros.Colaborador
            {
                Cpf = cpf,
                Nome = input.Nome,
                Email = input.Email,
                SetorId = input.SetorId,
                EmpresaId = input.EmpresaId
            };

            context.Colaboradores.Add(entity);

            try
            {
                await context.SaveChangesAsync(ct);
            }
            catch (DbUpdateException ex) when (IsUniqueConstraintViolation(ex))
            {
                throw new ArgumentException("Já existe um cadastro com este CPF.");
            }

            return entity;
        }

        /// <summary>
        /// Updates an existing colaborador.
        /// </summary>
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

            if (!CpfHelper.IsValidCpf(input.Cpf))
                throw new ArgumentException("CPF inválido");

            var cpf = CpfHelper.OnlyDigits(input.Cpf);

            var cpfAlreadyExists = await context.Colaboradores
                .AnyAsync(c => c.Cpf == cpf && c.Id != Id, ct);

            if (cpfAlreadyExists)
                throw new ArgumentException("Já existe um cadastro com este CPF.");

            entity.Cpf = cpf;
            entity.Nome = input.Nome;
            entity.Email = input.Email;
            entity.SetorId = input.SetorId;
            entity.EmpresaId = input.EmpresaId;
            entity.UpdatedAt = DateTime.UtcNow;

            try
            {
                await context.SaveChangesAsync(ct);
            }
            catch (DbUpdateException ex) when (IsUniqueConstraintViolation(ex))
            {
                throw new ArgumentException("Já existe um cadastro com este CPF.");
            }

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

        /// <summary>
        /// Detects PostgreSQL unique constraint violations (SQLSTATE 23505).
        /// </summary>
        private static bool IsUniqueConstraintViolation(DbUpdateException ex)
        {
            var message = ex.InnerException?.Message ?? ex.Message;
            return message.Contains("23505", StringComparison.Ordinal)
                || message.Contains("unique", StringComparison.OrdinalIgnoreCase)
                || message.Contains("IX_Colaboradores_Cpf", StringComparison.OrdinalIgnoreCase);
        }
    }
}
