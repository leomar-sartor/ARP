using ARP.Entity;
using ARP.Infra;
using ARP.Modules.Pessoa.Types;
using Microsoft.EntityFrameworkCore;

namespace ARP.Modules.Pessoa
{
    [ExtendObjectType("Mutation")]
    public class PessoaMutation
    {
        private readonly ILogger<PessoaMutation> _logger;

        public PessoaMutation(
            ILogger<PessoaMutation> logger
            )
        {
            _logger = logger;
        }

        [GraphQLDescription("Cadastrar um nova pessoa")]
        public async Task<Entity.Pessoa> CreatePessoa(
        PessoaInput input,
        [Service] Context context,
        CancellationToken ct)
        {
            var entity = new Entity.Pessoa
            {
                Nome = input.Nome
            };

            // 1. Pegar nomes únicos
            var nomes = input.Habilidades
                .Select(h => h.Nome.Trim().ToLower())
                .Distinct()
                .ToList();

            // 2. Buscar habilidades já existentes
            var habilidadesExistentes = await context.Habilidades
                .Where(h => nomes.Contains(h.Nome.ToLower()))
                //.Where(h => nomes.Contains(h.Nome.ToLower()) && h.DeletedAt == null)
                // não precisa pois existe o filtro
                .ToListAsync(ct);

            // 3. Criar dicionário
            var habilidadesDict = habilidadesExistentes
                .ToDictionary(h => h.Nome.ToLower(), h => h);

            foreach (var habilidadeInput in input.Habilidades)
            {
                var nome = habilidadeInput.Nome.Trim().ToLower();

                Habilidade habilidade;

                if (habilidadesDict.ContainsKey(nome))
                {
                    // Já existe
                    habilidade = habilidadesDict[nome];
                }
                else
                {
                    // Criar nova
                    habilidade = new Habilidade
                    {
                        Nome = habilidadeInput.Nome
                    };

                    context.Habilidades.Add(habilidade);

                    habilidadesDict[nome] = habilidade;
                }

                // Criar vínculo N:M
                entity.PessoaHabilidades.Add(new PessoaHabilidade
                {
                    Pessoa = entity,
                    Habilidade = habilidade
                });
            }

            foreach (var enderecoInput in input.Enderecos)
            {
                entity.Enderecos.Add(new Endereco
                {
                    Rua = enderecoInput.Rua,
                    Cidade = enderecoInput.Cidade,
                    CreatedAt = DateTime.UtcNow
                });
            }

            context.Pessoas.Add(entity);

            await context.SaveChangesAsync(ct);

            return entity;
        }

        [GraphQLDescription("Atualizar uma pessoa existente")]
        public async Task<Entity.Pessoa?> UpdatePessoa(
            long Id,
            PessoaInput input,
            [Service] Context context,
            CancellationToken ct)
        {
            var entity = await context.Pessoas
                   .Include(p => p.PessoaHabilidades)
                       .ThenInclude(ph => ph.Habilidade)
                   .FirstOrDefaultAsync(p => p.Id == Id, ct);

            if (entity == null)
                return null; //throw new Exception("Pessoa não encontrada");

            entity.Nome = input.Nome;

            // -------------------------------
            // NORMALIZA INPUT
            // -------------------------------
            var nomesInput = input.Habilidades
                .Select(h => h.Nome.Trim().ToLower())
                .Distinct()
                .ToList();

            // -------------------------------
            // EXISTENTES NO BANCO
            // -------------------------------
            var habilidadesExistentes = await context.Habilidades
                .Where(h => nomesInput.Contains(h.Nome.ToLower()))
                .ToListAsync(ct);

            var dictExistentes = habilidadesExistentes
            .ToDictionary(h => h.Nome.ToLower(), h => h);

            // -------------------------------
            // ATUAIS DA PESSOA
            // -------------------------------
            var atuais = entity.PessoaHabilidades
                .Select(ph => ph.Habilidade)
                .ToList();

            var atuaisNomes = atuais
                .Select(h => h.Nome.ToLower())
                .ToHashSet();

            // -------------------------------
            // NOVOS (ADD)
            // -------------------------------
            foreach (var nome in nomesInput)
            {
                if (!atuaisNomes.Contains(nome))
                {
                    Habilidade habilidade;

                    if (dictExistentes.ContainsKey(nome))
                    {
                        habilidade = dictExistentes[nome];
                    }
                    else
                    {
                        habilidade = new Habilidade
                        {
                            Nome = nome
                        };

                        context.Habilidades.Add(habilidade);
                        dictExistentes[nome] = habilidade;
                    }

                    entity.PessoaHabilidades.Add(new PessoaHabilidade
                    {
                        PessoaId = entity.Id,
                        Habilidade = habilidade
                    });
                }
            }

            // -------------------------------
            // REMOVER (DELETE)
            // -------------------------------
            var paraRemover = entity.PessoaHabilidades
                .Where(ph => !nomesInput.Contains(ph.Habilidade.Nome.ToLower()))
                .ToList();

            foreach (var item in paraRemover)
            {
                item.DeletedAt = DateTime.UtcNow;
            }

            await context.SaveChangesAsync();

            return entity;
        }

        [GraphQLDescription("Remover uma pessoa")]
        public async Task<bool> RemovePessoa(
            long id,
            [Service] Context context,
            CancellationToken ct)
        {
            await using var transaction = await context.Database.BeginTransactionAsync(ct);

            try
            {
                var entity = await context.Pessoas
                .Include(p => p.Enderecos)
                .Include(p => p.PessoaHabilidades)
                .FirstOrDefaultAsync(p => p.Id == id, ct);

                if (entity == null)
                    return false;

                var now = DateTime.UtcNow;
                entity.DeletedAt = now;

                // ------------------------
                // SOFT DELETE RELAÇÕES
                // ------------------------
                foreach (var rel in entity.PessoaHabilidades)
                {
                    rel.DeletedAt = now;
                }

                foreach (var end in entity.Enderecos)
                {
                    end.DeletedAt = now;
                }

                await context.SaveChangesAsync();

                await transaction.CommitAsync(ct);
            }
            catch (OperationCanceledException)
            {
                // cancelamento é esperado → não é erro
                throw;
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
