namespace TasteItApi.Graph.Requests
{
    public class CreateUserNodeRequest
    {
        public string? Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
    }
}
