using Neo4j.Driver;
using TasteItApi.Graph.Domain;
using TasteItApi.Graph.Testing.Dtos;

namespace TasteItApi.Graph.Testing.Repositories
{
    public class TestGraphRepository : ITestGraphRepository
    {
        private readonly IDriver _driver;

        public TestGraphRepository(IDriver driver)
        {
            _driver = driver;
        }

        public async Task SeedAsync(CancellationToken cancellationToken = default)
        {
            var users = new[]
            {
                new { Id = "test-user-1", Name = "Alice", Email = "alice@tasteit.test" },
                new { Id = "test-user-2", Name = "Bob", Email = "bob@tasteit.test" },
                new { Id = "test-user-3", Name = "Carla", Email = "carla@tasteit.test" }
            };

            var recipes = new[]
            {
                new { Id = "test-recipe-1", Name = "Pasta Roja", Description = "Pasta con tomate y albahaca", CreatedAt = DateTime.UtcNow, UserId = "test-user-1" },
                new { Id = "test-recipe-2", Name = "Pollo al Curry", Description = "Pollo con curry suave", CreatedAt = DateTime.UtcNow, UserId = "test-user-2" },
                new { Id = "test-recipe-3", Name = "Ensalada Fresca", Description = "Ensalada con tomate y pepino", CreatedAt = DateTime.UtcNow, UserId = "test-user-3" }
            };

            var ingredients = new[]
            {
                new { Id = "test-ing-1", Name = "Tomate", Type = "vegetable", RecipeId = "test-recipe-1" },
                new { Id = "test-ing-2", Name = "Albahaca", Type = "spice", RecipeId = "test-recipe-1" },
                new { Id = "test-ing-3", Name = "Pollo", Type = "meat", RecipeId = "test-recipe-2" },
                new { Id = "test-ing-4", Name = "Curry", Type = "spice", RecipeId = "test-recipe-2" },
                new { Id = "test-ing-5", Name = "Tomate", Type = "vegetable", RecipeId = "test-recipe-3" }
            };

            var likes = new[]
            {
                new { UserId = "test-user-1", RecipeId = "test-recipe-2" },
                new { UserId = "test-user-1", RecipeId = "test-recipe-3" },
                new { UserId = "test-user-2", RecipeId = "test-recipe-1" },
                new { UserId = "test-user-2", RecipeId = "test-recipe-3" },
                new { UserId = "test-user-3", RecipeId = "test-recipe-1" }
            };

            await using var session = _driver.AsyncSession();
            await session.ExecuteWriteAsync(async tx =>
            {
                await tx.RunAsync(@"
UNWIND $users AS user
MERGE (u:User {Id: user.Id})
SET u.Name = user.Name,
    u.Email = user.Email,
    u.CreatedAt = coalesce(u.CreatedAt, datetime())", new { users });

                await tx.RunAsync(@"
UNWIND $recipes AS recipe
MATCH (u:User {Id: recipe.UserId})
MERGE (r:Recipe {Id: recipe.Id})
SET r.Name = recipe.Name,
    r.Description = recipe.Description,
    r.CreatedAt = coalesce(r.CreatedAt, datetime(recipe.CreatedAt)),
    r.UpdatedAt = datetime(recipe.CreatedAt)
MERGE (u)-[:CREATED]->(r)", new { recipes });

                await tx.RunAsync(@"
UNWIND $ingredients AS ingredient
MATCH (r:Recipe {Id: ingredient.RecipeId})
MERGE (i:Ingredient {Id: ingredient.Id})
SET i.Name = ingredient.Name,
    i.Type = ingredient.Type,
    i.NameNormalized = toLower(ingredient.Name)
MERGE (r)-[c:CONTAINS]->(i)
SET c.Quantity = coalesce(c.Quantity, 1.0),
    c.Unit = coalesce(c.Unit, 'unit')", new { ingredients });

                await tx.RunAsync(@"
UNWIND $likes AS likeData
MATCH (u:User {Id: likeData.UserId})
MATCH (r:Recipe {Id: likeData.RecipeId})
MERGE (u)-[l:LIKES]->(r)
SET l.CreatedAt = coalesce(l.CreatedAt, datetime())", new { likes });
            });
        }

        public async Task CreateUserAsync(UserNode user, CancellationToken cancellationToken = default)
        {
            await using var session = _driver.AsyncSession();
            await session.ExecuteWriteAsync(async tx =>
            {
                await tx.RunAsync(@"
MERGE (u:User {Id: $id})
SET u.Name = $name, u.Email = $email, u.CreatedAt = coalesce(u.CreatedAt, datetime($createdAt))", new
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
                await tx.RunAsync(@"
MATCH (u:User {Id: $userId})
MERGE (r:Recipe {Id: $id})
SET r.Name = $name,
    r.Description = $description,
    r.CreatedAt = coalesce(r.CreatedAt, datetime($createdAt)),
    r.UpdatedAt = datetime($updatedAt)
MERGE (u)-[:CREATED]->(r)", new
                {
                    userId,
                    id = recipe.Id,
                    name = recipe.Name,
                    description = recipe.Description,
                    createdAt = recipe.CreatedAt,
                    updatedAt = recipe.UpdatedAt
                });
            });
        }

        public async Task<IngredientNode> AddIngredientToRecipeAsync(string recipeId, IngredientNode ingredient, CancellationToken cancellationToken = default)
        {
            await using var session = _driver.AsyncSession();
            await session.ExecuteWriteAsync(async tx =>
            {
                await tx.RunAsync(@"
MATCH (r:Recipe {Id: $recipeId})
MERGE (i:Ingredient {Id: $id})
SET i.Name = $name, i.Type = $type, i.NameNormalized = $nameNormalized
MERGE (r)-[c:CONTAINS]->(i)
SET c.Quantity = coalesce(c.Quantity, $quantity),
    c.Unit = coalesce(c.Unit, $unit)", new
                {
                    recipeId,
                    id = ingredient.Id,
                    name = ingredient.Name,
                    type = ingredient.Type,
                    nameNormalized = ingredient.NameNormalized,
                    quantity = 1.0,
                    unit = "unit"
                });
            });

            return ingredient;
        }

        public async Task LikeRecipeAsync(string recipeId, string userId, CancellationToken cancellationToken = default)
        {
            await using var session = _driver.AsyncSession();
            await session.ExecuteWriteAsync(async tx =>
            {
                await tx.RunAsync(@"
MATCH (u:User {Id: $userId})
MATCH (r:Recipe {Id: $recipeId})
MERGE (u)-[l:LIKES]->(r)
SET l.CreatedAt = coalesce(l.CreatedAt, datetime($createdAt))", new
                {
                    userId,
                    recipeId,
                    createdAt = DateTime.UtcNow
                });
            });
        }

        public async Task<IReadOnlyList<TestRecipeDto>> GetAllRecipesAsync(CancellationToken cancellationToken = default)
        {
            await using var session = _driver.AsyncSession();
            return await session.ExecuteReadAsync(async tx =>
            {
                var cursor = await tx.RunAsync(@"
MATCH (u:User)-[:CREATED|Created]->(r:Recipe)
RETURN r, u.Id AS userId
ORDER BY r.CreatedAt DESC
LIMIT 200");

                return await cursor.ToListAsync(record => new TestRecipeDto
                {
                    Id = GetNodeString(record["r"].As<INode>(), "Id"),
                    Name = GetNodeString(record["r"].As<INode>(), "Name"),
                    Description = GetNodeString(record["r"].As<INode>(), "Description"),
                    CreatedAt = GetNodeDateTime(record["r"].As<INode>(), "CreatedAt"),
                    CreatedByUserId = record["userId"].As<string>()
                });
            });
        }

        public async Task<IReadOnlyList<TestRecipeDto>> GetRecipesByIngredientAsync(string ingredientName, CancellationToken cancellationToken = default)
        {
            await using var session = _driver.AsyncSession();
            return await session.ExecuteReadAsync(async tx =>
            {
                var cursor = await tx.RunAsync(@"
MATCH (u:User)-[:CREATED|Created]->(r:Recipe)-[:CONTAINS|Contains]->(i:Ingredient)
WHERE toLower(coalesce(i.NameNormalized, i.Name, '')) CONTAINS toLower($ingredientName)
RETURN DISTINCT r, u.Id AS userId
ORDER BY r.CreatedAt DESC
LIMIT 200", new { ingredientName });

                return await cursor.ToListAsync(record => new TestRecipeDto
                {
                    Id = GetNodeString(record["r"].As<INode>(), "Id"),
                    Name = GetNodeString(record["r"].As<INode>(), "Name"),
                    Description = GetNodeString(record["r"].As<INode>(), "Description"),
                    CreatedAt = GetNodeDateTime(record["r"].As<INode>(), "CreatedAt"),
                    CreatedByUserId = record["userId"].As<string>()
                });
            });
        }

        public async Task<IReadOnlyList<TestRecipeDto>> GetRecipesByUserAsync(string userId, CancellationToken cancellationToken = default)
        {
            await using var session = _driver.AsyncSession();
            return await session.ExecuteReadAsync(async tx =>
            {
                var cursor = await tx.RunAsync(@"
MATCH (u:User {Id: $userId})-[:CREATED|Created]->(r:Recipe)
RETURN r, u.Id AS creatorId
ORDER BY r.CreatedAt DESC
LIMIT 200", new { userId });

                return await cursor.ToListAsync(record => new TestRecipeDto
                {
                    Id = GetNodeString(record["r"].As<INode>(), "Id"),
                    Name = GetNodeString(record["r"].As<INode>(), "Name"),
                    Description = GetNodeString(record["r"].As<INode>(), "Description"),
                    CreatedAt = GetNodeDateTime(record["r"].As<INode>(), "CreatedAt"),
                    CreatedByUserId = record["creatorId"].As<string>()
                });
            });
        }

        public async Task<IReadOnlyList<TestIngredientDto>> GetIngredientsOfRecipeAsync(string recipeId, CancellationToken cancellationToken = default)
        {
            await using var session = _driver.AsyncSession();
            return await session.ExecuteReadAsync(async tx =>
            {
                var cursor = await tx.RunAsync(@"
MATCH (r:Recipe {Id: $recipeId})-[c:CONTAINS|Contains]->(i:Ingredient)
RETURN i, c
ORDER BY i.Name
LIMIT 200", new { recipeId });

                return await cursor.ToListAsync(record =>
                {
                    var node = record["i"].As<INode>();
                    var rel = record["c"].As<IRelationship>();
                    return new TestIngredientDto
                    {
                        Id = GetNodeString(node, "Id"),
                        Name = GetNodeString(node, "Name"),
                        Type = GetNodeString(node, "Type"),
                        Quantity = rel.Properties.TryGetValue("Quantity", out var qty) ? Convert.ToDouble(qty) : 0,
                        Unit = rel.Properties.TryGetValue("Unit", out var unit) ? unit?.ToString() ?? string.Empty : string.Empty
                    };
                });
            });
        }

        public async Task<IReadOnlyList<TestRecommendationDto>> GetRecommendationsAsync(string userId, CancellationToken cancellationToken = default)
        {
            await using var session = _driver.AsyncSession();
            return await session.ExecuteReadAsync(async tx =>
            {
                var cursor = await tx.RunAsync(@"
MATCH (u:User {Id: $userId})-[:LIKES|Liked]->(:Recipe)<-[:LIKES|Liked]-(other:User)-[:LIKES|Liked]->(recommended:Recipe)
WHERE u <> other AND NOT (u)-[:LIKES|Liked]->(recommended)
RETURN recommended, count(DISTINCT other) AS score
ORDER BY score DESC, recommended.CreatedAt DESC
LIMIT $limit", new { userId, limit = 30 });

                return await cursor.ToListAsync(record =>
                {
                    var recipeNode = record["recommended"].As<INode>();
                    return new TestRecommendationDto
                    {
                        RecipeId = GetNodeString(recipeNode, "Id"),
                        Name = GetNodeString(recipeNode, "Name"),
                        Description = GetNodeString(recipeNode, "Description"),
                        Score = record["score"].As<int>()
                    };
                });
            });
        }

        public async Task<GraphSnapshotDto> GetFullGraphAsync(CancellationToken cancellationToken = default)
        {
            await using var session = _driver.AsyncSession();
            return await session.ExecuteReadAsync(async tx =>
            {
                var nodeCursor = await tx.RunAsync("MATCH (n) RETURN n LIMIT 1000");
                var relationshipCursor = await tx.RunAsync("MATCH ()-[r]->() RETURN r LIMIT 1000");

                var nodes = await nodeCursor.ToListAsync(record =>
                {
                    var node = record["n"].As<INode>();
                    return new GraphNodeDto
                    {
                        ElementId = node.ElementId,
                        Labels = node.Labels.ToList(),
                        Properties = node.Properties.ToDictionary(x => x.Key, x => (object?)x.Value)
                    };
                });

                var relationships = await relationshipCursor.ToListAsync(record =>
                {
                    var rel = record["r"].As<IRelationship>();
                    return new GraphRelationshipDto
                    {
                        ElementId = rel.ElementId,
                        Type = rel.Type,
                        StartNodeElementId = rel.StartNodeElementId,
                        EndNodeElementId = rel.EndNodeElementId,
                        Properties = rel.Properties.ToDictionary(x => x.Key, x => (object?)x.Value)
                    };
                });

                return new GraphSnapshotDto
                {
                    Nodes = nodes,
                    Relationships = relationships
                };
            });
        }

        private static string GetNodeString(INode node, string key)
        {
            return node.Properties.TryGetValue(key, out var value)
                ? value?.ToString() ?? string.Empty
                : string.Empty;
        }

        private static DateTime GetNodeDateTime(INode node, string key)
        {
            if (!node.Properties.TryGetValue(key, out var value) || value is null)
            {
                return default;
            }

            return value switch
            {
                DateTime dt => dt,
                DateTimeOffset dto => dto.UtcDateTime,
                LocalDateTime ldt => ldt.ToDateTime(),
                ZonedDateTime zdt => zdt.ToDateTimeOffset().UtcDateTime,
                _ => DateTime.TryParse(value.ToString(), out var parsed) ? parsed : default
            };
        }
    }
}
