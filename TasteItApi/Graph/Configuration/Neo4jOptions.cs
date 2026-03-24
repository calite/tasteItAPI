namespace TasteItApi.Graph.Configuration
{
    public class Neo4jOptions
    {
        public string Uri { get; set; } = "bolt://localhost:7687";
        public string Username { get; set; } = "neo4j";
        public string Password { get; set; } = string.Empty;
        public int ConnectionTimeoutSeconds { get; set; } = 15;
        public int MaxConnectionPoolSize { get; set; } = 100;
    }
}
