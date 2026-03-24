namespace TasteItApi.Graph.Requests
{
    public class CreateRecipeNodeRequest
    {
        public string? Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string CreatedByUserId { get; set; } = string.Empty;
    }
}
