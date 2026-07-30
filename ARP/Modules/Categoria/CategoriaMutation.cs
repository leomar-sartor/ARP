using ARP.Infra;
using ARP.Modules.Categoria.Types;
using Microsoft.EntityFrameworkCore;

namespace ARP.Modules.Categoria
{
    [ExtendObjectType("Mutation")]
    public class CategoriaMutation
    {
        private readonly ILogger<CategoriaMutation> _logger;

        public CategoriaMutation(ILogger<CategoriaMutation> logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// Creates a new categoria.
        /// </summary>
        [GraphQLDescription("Cadastrar uma nova categoria")]
        public async Task<Entity.Pesquisas.Categoria> CreateCategoria(
            CategoriaInput input,
            [Service] Context context)
        {
            var entity = new Entity.Pesquisas.Categoria
            {
                Nome = input.Nome,
                Descricao = input.Descricao,
                Ativo = true
            };

            context.Categorias.Add(entity);
            await context.SaveChangesAsync();
            return entity;
        }

        /// <summary>
        /// Updates an existing categoria.
        /// </summary>
        [GraphQLDescription("Atualizar uma categoria existente")]
        public async Task<Entity.Pesquisas.Categoria?> UpdateCategoria(
            long id,
            CategoriaInput input,
            [Service] Context context)
        {
            var entity = await context.Categorias.FindAsync(id);

            if (entity is null)
                return null;

            entity.Nome = input.Nome;
            entity.Descricao = input.Descricao;
            entity.UpdatedAt = DateTime.UtcNow;

            await context.SaveChangesAsync();
            return entity;
        }

        /// <summary>
        /// Soft-deletes a categoria when it has no linked questoes.
        /// </summary>
        [GraphQLDescription("Remover uma categoria")]
        public async Task<bool> RemoveCategoria(
            long id,
            [Service] Context context)
        {
            var entity = await context.Categorias.FindAsync(id);

            if (entity is null)
                return false;

            var hasQuestoes = await context.Questoes.AnyAsync(q => q.CategoriaId == id);
            if (hasQuestoes)
                throw new ArgumentException(
                    "Não é possível remover uma categoria que possui questões associadas.");

            entity.DeletedAt = DateTime.UtcNow;
            await context.SaveChangesAsync();
            return true;
        }
    }
}
