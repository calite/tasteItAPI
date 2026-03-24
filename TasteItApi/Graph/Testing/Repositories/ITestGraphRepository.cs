using TasteItApi.Graph.Domain;
using TasteItApi.Graph.Testing.Dtos;

namespace TasteItApi.Graph.Testing.Repositories
{
    public interface ITestGraphRepository
    {
        Task SeedAsync(CancellationToken cancellationToken = default);
        Task CreateUserAsync(UserNode user, CancellationToken cancellationToken = default);
        Task CreateRecipeAsync(RecipeNode recipe, string userId, CancellationToken cancellationToken = default);
        Task<IngredientNode> AddIngredientToRecipeAsync(string recipeId, IngredientNode ingredient, CancellationToken cancellationToken = default);
        Task LikeRecipeAsync(string recipeId, string userId, CancellationToken cancellationToken = default);
        Task<IReadOnlyList<TestRecipeDto>> GetAllRecipesAsync(CancellationToken cancellationToken = default);
        Task<IReadOnlyList<TestRecipeDto>> GetRecipesByIngredientAsync(string ingredientName, CancellationToken cancellationToken = default);
        Task<IReadOnlyList<TestRecipeDto>> GetRecipesByUserAsync(string userId, CancellationToken cancellationToken = default);
        Task<IReadOnlyList<TestIngredientDto>> GetIngredientsOfRecipeAsync(string recipeId, CancellationToken cancellationToken = default);
        Task<IReadOnlyList<TestRecommendationDto>> GetRecommendationsAsync(string userId, CancellationToken cancellationToken = default);
        Task<GraphSnapshotDto> GetFullGraphAsync(CancellationToken cancellationToken = default);
    }
}
