using TasteItApi.Graph.Testing.Dtos;

namespace TasteItApi.Graph.Testing.Services
{
    public interface ITestGraphService
    {
        Task<SeedResultDto> SeedAsync(CancellationToken cancellationToken = default);
        Task<string> CreateUserAsync(CreateTestUserRequest request, CancellationToken cancellationToken = default);
        Task<string> CreateRecipeAsync(CreateTestRecipeRequest request, CancellationToken cancellationToken = default);
        Task<TestIngredientDto> AddIngredientToRecipeAsync(string recipeId, AddTestIngredientRequest request, CancellationToken cancellationToken = default);
        Task LikeRecipeAsync(string recipeId, string userId, CancellationToken cancellationToken = default);
        Task<IReadOnlyList<TestRecipeDto>> GetAllRecipesAsync(CancellationToken cancellationToken = default);
        Task<IReadOnlyList<TestRecipeDto>> GetRecipesByIngredientAsync(string ingredientName, CancellationToken cancellationToken = default);
        Task<IReadOnlyList<TestRecipeDto>> GetRecipesByUserAsync(string userId, CancellationToken cancellationToken = default);
        Task<IReadOnlyList<TestIngredientDto>> GetIngredientsOfRecipeAsync(string recipeId, CancellationToken cancellationToken = default);
        Task<IReadOnlyList<TestRecommendationDto>> GetRecommendationsAsync(string userId, CancellationToken cancellationToken = default);
        Task<GraphSnapshotDto> GetFullGraphAsync(CancellationToken cancellationToken = default);
    }
}
