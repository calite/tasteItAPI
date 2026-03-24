using TasteItApi.Graph.Domain;

namespace TasteItApi.Graph.Services
{
    public interface IRecipeGraphService
    {
        Task<UserNode> CreateUserAsync(string? id, string name, string email, CancellationToken cancellationToken = default);
        Task<RecipeNode> CreateRecipeAsync(string? id, string name, string description, string createdByUserId, CancellationToken cancellationToken = default);
        Task<IngredientNode> AddIngredientToRecipeAsync(string recipeId, string? ingredientId, string ingredientName, string ingredientType, CancellationToken cancellationToken = default);
        Task AddLikeToRecipeAsync(string userId, string recipeId, CancellationToken cancellationToken = default);
    }
}
