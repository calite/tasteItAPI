using Neo4j.Driver;
using TasteItApi.Graph.Admin.Dtos;
using TasteItApi.Models;

namespace TasteItApi.Graph.Admin.Repositories
{
    public class AdminGraphRepository : IAdminGraphRepository
    {
        private readonly IDriver _driver;

        public AdminGraphRepository(IDriver driver)
        {
            _driver = driver;
        }

        public async Task<IReadOnlyList<AdminReportedRecipeDto>> GetRecipesReportedAsync(CancellationToken cancellationToken = default)
        {
            const string cypher = @"
MATCH (recipe:Recipe)<-[:CREATED|Created]-(u1:User)
OPTIONAL MATCH (recipe)-[report:REPORTED|Reported]-(:User)
RETURN ID(recipe) AS recipeId, recipe, u1 AS creator, count(report) AS reportsCount
ORDER BY reportsCount DESC";

            await using var session = _driver.AsyncSession();
            return await session.ExecuteReadAsync(async tx =>
            {
                var cursor = await tx.RunAsync(cypher);
                return await cursor.ToListAsync(record => new AdminReportedRecipeDto
                {
                    recipeId = record["recipeId"].As<long>(),
                    recipe = MapRecipeWeb(record["recipe"].As<INode>(), record["recipeId"].As<long>()),
                    creator = MapUser(record["creator"].As<INode>()),
                    reportsCount = record["reportsCount"].As<long>()
                });
            });
        }

        public async Task<IReadOnlyList<AdminReportedRecipeDto>> GetRecipesReportedFilteredAsync(string? name, string? creator, bool? active, CancellationToken cancellationToken = default)
        {
            const string cypher = @"
MATCH (recipe:Recipe)<-[:CREATED|Created]-(u1:User)
WHERE ($name IS NULL OR toLower(coalesce(recipe.Name, recipe.name, '')) CONTAINS toLower($name))
  AND ($creator IS NULL OR toLower(coalesce(u1.Name, u1.username, '')) CONTAINS toLower($creator))
  AND ($active IS NULL OR coalesce(recipe.active, true) = $active)
OPTIONAL MATCH (recipe)-[report:REPORTED|Reported]-(:User)
RETURN ID(recipe) AS recipeId, recipe, u1 AS creator, count(report) AS reportsCount
ORDER BY reportsCount DESC";

            await using var session = _driver.AsyncSession();
            return await session.ExecuteReadAsync(async tx =>
            {
                var cursor = await tx.RunAsync(cypher, new { name, creator, active });
                return await cursor.ToListAsync(record => new AdminReportedRecipeDto
                {
                    recipeId = record["recipeId"].As<long>(),
                    recipe = MapRecipeWeb(record["recipe"].As<INode>(), record["recipeId"].As<long>()),
                    creator = MapUser(record["creator"].As<INode>()),
                    reportsCount = record["reportsCount"].As<long>()
                });
            });
        }

        public async Task<IReadOnlyList<AdminRecipeByIdDto>> GetRecipeReportedByIdAsync(int id, CancellationToken cancellationToken = default)
        {
            const string cypher = @"
MATCH (user:User)-[:CREATED|Created]->(recipe:Recipe)
WHERE ID(recipe) = $id
RETURN ID(recipe) AS recipeId, recipe, user";

            await using var session = _driver.AsyncSession();
            return await session.ExecuteReadAsync(async tx =>
            {
                var cursor = await tx.RunAsync(cypher, new { id });
                return await cursor.ToListAsync(record => new AdminRecipeByIdDto
                {
                    RecipeId = record["recipeId"].As<long>(),
                    Recipe = MapRecipeWeb(record["recipe"].As<INode>(), record["recipeId"].As<long>()),
                    User = MapUser(record["user"].As<INode>())
                });
            });
        }

        public async Task<IReadOnlyList<RecipeWEB>> ChangeRecipeStateAsync(int recipeId, bool value, CancellationToken cancellationToken = default)
        {
            const string cypher = @"
MATCH (recipe:Recipe)
WHERE ID(recipe) = $recipeId
SET recipe.active = $value, recipe.UpdatedAt = datetime($updatedAt)
RETURN ID(recipe) AS recipeId, recipe";

            await using var session = _driver.AsyncSession();
            return await session.ExecuteWriteAsync(async tx =>
            {
                var cursor = await tx.RunAsync(cypher, new
                {
                    recipeId,
                    value,
                    updatedAt = DateTime.UtcNow
                });
                return await cursor.ToListAsync(record => MapRecipeWeb(record["recipe"].As<INode>(), record["recipeId"].As<long>()));
            });
        }

        public async Task<IReadOnlyList<AdminReportDto>> GetReportsOnRecipeAsync(int id, CancellationToken cancellationToken = default)
        {
            const string cypher = @"
MATCH (user:User)-[report:REPORTED|Reported]-(recipe:Recipe)
WHERE ID(recipe) = $id
RETURN user, report";

            await using var session = _driver.AsyncSession();
            return await session.ExecuteReadAsync(async tx =>
            {
                var cursor = await tx.RunAsync(cypher, new { id });
                return await cursor.ToListAsync(record => new AdminReportDto
                {
                    User = MapUser(record["user"].As<INode>()),
                    Report = MapReport(record["report"].As<IRelationship>())
                });
            });
        }

        private static User MapUser(INode node)
        {
            return new User
            {
                token = GetString(node, "Id", "token"),
                username = GetString(node, "Name", "username"),
                imgProfile = GetString(node, "imgProfile"),
                biography = GetString(node, "biography")
            };
        }

        private static RecipeWEB MapRecipeWeb(INode node, long id)
        {
            return new RecipeWEB
            {
                Id = (int)id,
                name = GetString(node, "Name", "name"),
                description = GetString(node, "Description", "description"),
                difficulty = GetInt(node, "difficulty"),
                image = GetString(node, "image"),
                dateCreated = GetString(node, "CreatedAt", "dateCreated"),
                country = GetString(node, "country"),
                rating = GetFloat(node, "rating"),
                ingredients = GetStringList(node, "ingredients"),
                tags = GetStringList(node, "tags"),
                steps = GetStringList(node, "steps"),
                active = GetBool(node, "active", true)
            };
        }

        private static Report MapReport(IRelationship relationship)
        {
            return new Report
            {
                comment = relationship.Properties.TryGetValue("comment", out var comment)
                    ? comment?.ToString() ?? string.Empty
                    : string.Empty,
                dateCreated = relationship.Properties.TryGetValue("dateCreated", out var date)
                    ? date.As<DateTimeOffset>()
                    : default
            };
        }

        private static string GetString(INode node, params string[] keys)
        {
            foreach (var key in keys)
            {
                if (node.Properties.TryGetValue(key, out var value) && value is not null)
                {
                    return value.ToString() ?? string.Empty;
                }
            }

            return string.Empty;
        }

        private static int GetInt(INode node, string key)
        {
            return node.Properties.TryGetValue(key, out var value) ? Convert.ToInt32(value) : 0;
        }

        private static float GetFloat(INode node, string key)
        {
            return node.Properties.TryGetValue(key, out var value) ? Convert.ToSingle(value) : 0f;
        }

        private static bool GetBool(INode node, string key, bool defaultValue = false)
        {
            return node.Properties.TryGetValue(key, out var value) ? Convert.ToBoolean(value) : defaultValue;
        }

        private static List<string> GetStringList(INode node, string key)
        {
            if (!node.Properties.TryGetValue(key, out var value) || value is null)
            {
                return new List<string>();
            }

            if (value is IEnumerable<object> objects)
            {
                return objects.Select(x => x?.ToString() ?? string.Empty).ToList();
            }

            if (value is IEnumerable<string> strings)
            {
                return strings.ToList();
            }

            return new List<string>();
        }
    }
}
