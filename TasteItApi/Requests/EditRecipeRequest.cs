namespace TasteItApi.Requests
{
    public class EditRecipeRequest
    {
        public int rid { get; set; }
        public string name { get; set; } = string.Empty;
        public string description { get; set; } = string.Empty;
        public string country { get; set; } = string.Empty;
        public string image { get; set; } = string.Empty;
        public int difficulty { get; set; }
        public string[] ingredients { get; set; } = Array.Empty<string>();
        public string[] steps { get; set; } = Array.Empty<string>();
        //public string tags { get; set; } = string.Empty;
    }
}

