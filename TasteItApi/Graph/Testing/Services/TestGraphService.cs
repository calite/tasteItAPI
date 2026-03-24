using TasteItApi.Graph.Domain;
using TasteItApi.Graph.Testing.Dtos;
using TasteItApi.Graph.Testing.Repositories;

namespace TasteItApi.Graph.Testing.Services
{
    public class TestGraphService : ITestGraphService
    {
        private readonly ITestGraphRepository _repository;

        public TestGraphService(ITestGraphRepository repository)
        {
            _repository = repository;
        }

        public async Task<SeedResultDto> SeedAsync(CancellationToken cancellationToken = default)
        {
            await _repository.SeedAsync(cancellationToken);
            return new SeedResultDto
            {
                Users = 3,
                Recipes = 3,
                Ingredients = 5,
                Likes = 5
            };
        }

        public async Task<string> CreateUserAsync(CreateTestUserRequest request, CancellationToken cancellationToken = default)
        {
            var id = Guid.NewGuid().ToString("N");
            await _repository.CreateUserAsync(new UserNode
            {
                Id = id,
                Name = request.Name.Trim(),
                Email = request.Email.Trim(),
                CreatedAt = DateTime.UtcNow
            }, cancellationToken);

            return id;
        }

        public async Task<string> CreateRecipeAsync(CreateTestRecipeRequest request, CancellationToken cancellationToken = default)
        {
            var id = Guid.NewGuid().ToString("N");
            await _repository.CreateRecipeAsync(new RecipeNode
            {
                Id = id,
                Name = request.Name.Trim(),
                Description = request.Description.Trim(),
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            }, request.UserId, cancellationToken);

            return id;
        }

        public async Task<TestIngredientDto> AddIngredientToRecipeAsync(string recipeId, AddTestIngredientRequest request, CancellationToken cancellationToken = default)
        {
            var ingredient = await _repository.AddIngredientToRecipeAsync(recipeId, new IngredientNode
            {
                Id = Guid.NewGuid().ToString("N"),
                Name = request.IngredientName.Trim(),
                Type = request.Type.Trim(),
                NameNormalized = request.IngredientName.Trim().ToLowerInvariant()
            }, cancellationToken);

            return new TestIngredientDto
            {
                Id = ingredient.Id,
                Name = ingredient.Name,
                Type = ingredient.Type,
                Quantity = 1,
                Unit = "unit"
            };
        }

        public Task LikeRecipeAsync(string recipeId, string userId, CancellationToken cancellationToken = default)
        {
            return _repository.LikeRecipeAsync(recipeId, userId, cancellationToken);
        }

        public Task<IReadOnlyList<TestRecipeDto>> GetAllRecipesAsync(CancellationToken cancellationToken = default)
        {
            return _repository.GetAllRecipesAsync(cancellationToken);
        }

        public Task<IReadOnlyList<TestRecipeDto>> GetRecipesByIngredientAsync(string ingredientName, CancellationToken cancellationToken = default)
        {
            return _repository.GetRecipesByIngredientAsync(ingredientName, cancellationToken);
        }

        public Task<IReadOnlyList<TestRecipeDto>> GetRecipesByUserAsync(string userId, CancellationToken cancellationToken = default)
        {
            return _repository.GetRecipesByUserAsync(userId, cancellationToken);
        }

        public Task<IReadOnlyList<TestIngredientDto>> GetIngredientsOfRecipeAsync(string recipeId, CancellationToken cancellationToken = default)
        {
            return _repository.GetIngredientsOfRecipeAsync(recipeId, cancellationToken);
        }

        public Task<IReadOnlyList<TestRecommendationDto>> GetRecommendationsAsync(string userId, CancellationToken cancellationToken = default)
        {
            return _repository.GetRecommendationsAsync(userId, cancellationToken);
        }

        public Task<GraphSnapshotDto> GetFullGraphAsync(CancellationToken cancellationToken = default)
        {
            return _repository.GetFullGraphAsync(cancellationToken);
        }
    }
}
