using Neo4j.Driver;
using TasteItApi.Graph.Domain;

namespace TasteItApi.Graph.Repositories
{
    public class RecipeGraphRepository : IRecipeGraphRepository
    {
        private const string CreateUserCypher = @"
MERGE (u:User {Id: $id})
SET u.Name = $name,
    u.Email = $email";

        private const string CreateRecipeAndLinkCypher = @"
MATCH (u:User {Id: $userId})
MERGE (r:Recipe {Id: $recipeId})
SET r.Name = $name,
    r.Description = $description,
    r.CreatedAt = $createdAt
MERGE (u)-[:CREATED]->(r)";

        private const string AddIngredientCypher = @"
MATCH (r:Recipe {Id: $recipeId})
MERGE (i:Ingredient {Id: $ingredientId})
SET i.Name = $ingredientName,
    i.Type = $ingredientType
MERGE (r)-[:CONTAINS]->(i)";

        private const string AddLikeCypher = @"
MATCH (u:User {Id: $userId})
MATCH (r:Recipe {Id: $recipeId})
MERGE (u)-[:LIKES]->(r)";

        private readonly IDriver _driver;

        public RecipeGraphRepository(IDriver driver)
        {
            _driver = driver;
        }

        public async Task CreateUserAsync(UserNode user, CancellationToken cancellationToken = default)
        {
            await using var session = _driver.AsyncSession();
            await session.ExecuteWriteAsync(async tx =>
            {
                await tx.RunAsync(CreateUserCypher, new
                {
                    id = user.Id,
                    name = user.Name,
                    email = user.Email
                });
            });
        }

        public async Task CreateRecipeAsync(RecipeNode recipe, string userId, CancellationToken cancellationToken = default)
        {
            await using var session = _driver.AsyncSession();
            await session.ExecuteWriteAsync(async tx =>
            {
                await tx.RunAsync(CreateRecipeAndLinkCypher, new
                {
                    userId,
                    recipeId = recipe.Id,
                    name = recipe.Name,
                    description = recipe.Description,
                    createdAt = recipe.CreatedAt
                });
            });
        }

        public async Task AddIngredientToRecipeAsync(string recipeId, IngredientNode ingredient, CancellationToken cancellationToken = default)
        {
            await using var session = _driver.AsyncSession();
            await session.ExecuteWriteAsync(async tx =>
            {
                await tx.RunAsync(AddIngredientCypher, new
                {
                    recipeId,
                    ingredientId = ingredient.Id,
                    ingredientName = ingredient.Name,
                    ingredientType = ingredient.Type
                });
            });
        }

        public async Task AddLikeToRecipeAsync(string userId, string recipeId, CancellationToken cancellationToken = default)
        {
            await using var session = _driver.AsyncSession();
            await session.ExecuteWriteAsync(async tx =>
            {
                await tx.RunAsync(AddLikeCypher, new
                {
                    userId,
                    recipeId
                });
            });
        }
    }
}
