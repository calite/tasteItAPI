using Neo4j.Driver;
using TasteItApi.Graph.Domain;

namespace TasteItApi.Graph.Repositories
{
    public class RecipeGraphRepository : IRecipeGraphRepository
    {
        private const string CreateUserCypher = @"
MERGE (u:User {Id: $id})
SET u.Name = $name,
    u.Email = $email,
    u.CreatedAt = coalesce(u.CreatedAt, datetime($createdAt))";

        private const string CreateRecipeAndLinkCypher = @"
MATCH (u:User)
WHERE u.Id = $userId OR u.token = $userId
MERGE (r:Recipe {Id: $recipeId})
SET r.Name = $name,
    r.Description = $description,
    r.CreatedAt = coalesce(r.CreatedAt, datetime($createdAt)),
    r.UpdatedAt = datetime($updatedAt)
MERGE (u)-[:CREATED]->(r)";

        private const string AddIngredientCypher = @"
MATCH (r:Recipe)
WHERE r.Id = $recipeId OR toString(ID(r)) = $recipeId
MERGE (i:Ingredient {Id: $ingredientId})
SET i.Name = $ingredientName,
    i.Type = $ingredientType,
    i.NameNormalized = $ingredientNameNormalized
MERGE (r)-[c:CONTAINS]->(i)
SET c.Quantity = coalesce(c.Quantity, $quantity),
    c.Unit = coalesce(c.Unit, $unit)";

        private const string AddLikeCypher = @"
MATCH (u:User)
WHERE u.Id = $userId OR u.token = $userId
MATCH (r:Recipe)
WHERE r.Id = $recipeId OR toString(ID(r)) = $recipeId
MERGE (u)-[l:LIKES]->(r)
SET l.CreatedAt = coalesce(l.CreatedAt, datetime($createdAt))";

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
                    email = user.Email,
                    createdAt = user.CreatedAt
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
                    createdAt = recipe.CreatedAt,
                    updatedAt = recipe.UpdatedAt
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
                    ingredientType = ingredient.Type,
                    ingredientNameNormalized = ingredient.NameNormalized,
                    quantity = 1.0,
                    unit = "unit"
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
                    recipeId,
                    createdAt = DateTime.UtcNow
                });
            });
        }
    }
}
