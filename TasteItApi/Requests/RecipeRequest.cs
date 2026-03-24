namespace TasteItApi.Requests
{
    public class RecipeRequest
    {
        public string token { get; set; } = string.Empty;
        public string name { get; set; } = string.Empty;
        public string description { get; set; } = string.Empty;
        public string country { get; set; } = string.Empty;
        public string image { get; set; } = string.Empty;
        public int difficulty { get; set; }
        public string[] ingredients { get; set; } = Array.Empty<string>();
        public string[] steps { get; set; } = Array.Empty<string>();

    }
}

