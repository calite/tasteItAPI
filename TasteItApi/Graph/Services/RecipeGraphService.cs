using TasteItApi.Graph.Domain;
using TasteItApi.Graph.Repositories;

namespace TasteItApi.Graph.Services
{
    public class RecipeGraphService : IRecipeGraphService
    {
        private readonly IRecipeGraphRepository _repository;

        public RecipeGraphService(IRecipeGraphRepository repository)
        {
            _repository = repository;
        }

        public async Task<UserNode> CreateUserAsync(string? id, string name, string email, CancellationToken cancellationToken = default)
        {
            var user = new UserNode
            {
                Id = string.IsNullOrWhiteSpace(id) ? Guid.NewGuid().ToString("N") : id,
                Name = name.Trim(),
                Email = email.Trim(),
                CreatedAt = DateTime.UtcNow
            };

            await _repository.CreateUserAsync(user, cancellationToken);
            return user;
        }

        public async Task<RecipeNode> CreateRecipeAsync(string? id, string name, string description, string createdByUserId, CancellationToken cancellationToken = default)
        {
            var recipe = new RecipeNode
            {
                Id = string.IsNullOrWhiteSpace(id) ? Guid.NewGuid().ToString("N") : id,
                Name = name.Trim(),
                Description = description.Trim(),
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            await _repository.CreateRecipeAsync(recipe, createdByUserId, cancellationToken);
            return recipe;
        }

        public async Task<IngredientNode> AddIngredientToRecipeAsync(string recipeId, string? ingredientId, string ingredientName, string ingredientType, CancellationToken cancellationToken = default)
        {
            var ingredient = new IngredientNode
            {
                Id = string.IsNullOrWhiteSpace(ingredientId) ? Guid.NewGuid().ToString("N") : ingredientId,
                Name = ingredientName.Trim(),
                Type = ingredientType.Trim(),
                NameNormalized = ingredientName.Trim().ToLowerInvariant()
            };

            await _repository.AddIngredientToRecipeAsync(recipeId, ingredient, cancellationToken);
            return ingredient;
        }

        public Task AddLikeToRecipeAsync(string userId, string recipeId, CancellationToken cancellationToken = default)
        {
            return _repository.AddLikeToRecipeAsync(userId, recipeId, cancellationToken);
        }
    }
}
