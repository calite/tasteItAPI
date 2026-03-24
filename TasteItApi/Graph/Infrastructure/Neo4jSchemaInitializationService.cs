using Neo4j.Driver;

namespace TasteItApi.Graph.Infrastructure
{
    public class Neo4jSchemaInitializationService : IHostedService
    {
        private readonly IDriver _driver;
        private readonly ILogger<Neo4jSchemaInitializationService> _logger;

        public Neo4jSchemaInitializationService(IDriver driver, ILogger<Neo4jSchemaInitializationService> logger)
        {
            _driver = driver;
            _logger = logger;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            var statements = new[]
            {
                "CREATE CONSTRAINT user_id_unique IF NOT EXISTS FOR (u:User) REQUIRE u.Id IS UNIQUE",
                "CREATE CONSTRAINT recipe_id_unique IF NOT EXISTS FOR (r:Recipe) REQUIRE r.Id IS UNIQUE",
                "CREATE CONSTRAINT ingredient_id_unique IF NOT EXISTS FOR (i:Ingredient) REQUIRE i.Id IS UNIQUE",
                "CREATE INDEX ingredient_name_idx IF NOT EXISTS FOR (i:Ingredient) ON (i.Name)",
                "CREATE INDEX recipe_name_idx IF NOT EXISTS FOR (r:Recipe) ON (r.Name)"
            };

            await using var session = _driver.AsyncSession();
            await session.ExecuteWriteAsync(async tx =>
            {
                foreach (var statement in statements)
                {
                    await tx.RunAsync(statement);
                }
            });

            _logger.LogInformation("Neo4j schema initialized.");
        }

        public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
    }
}
