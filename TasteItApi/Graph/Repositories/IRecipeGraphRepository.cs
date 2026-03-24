using TasteItApi.Graph.Domain;

namespace TasteItApi.Graph.Repositories
{
    public interface IRecipeGraphRepository
    {
        Task CreateUserAsync(UserNode user, CancellationToken cancellationToken = default);
        Task CreateRecipeAsync(RecipeNode recipe, string userId, CancellationToken cancellationToken = default);
        Task AddIngredientToRecipeAsync(string recipeId, IngredientNode ingredient, CancellationToken cancellationToken = default);
        Task AddLikeToRecipeAsync(string userId, string recipeId, CancellationToken cancellationToken = default);
    }
}
