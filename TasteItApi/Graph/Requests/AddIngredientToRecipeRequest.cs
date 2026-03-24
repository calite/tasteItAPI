namespace TasteItApi.Graph.Requests
{
    public class AddIngredientToRecipeRequest
    {
        public string RecipeId { get; set; } = string.Empty;
        public string? IngredientId { get; set; }
        public string IngredientName { get; set; } = string.Empty;
        public string IngredientType { get; set; } = string.Empty;
    }
}
