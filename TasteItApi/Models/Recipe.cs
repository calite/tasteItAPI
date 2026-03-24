namespace TasteItApi.Models
{
    public class Recipe
    {
        public int Id { get; set; }
        public string name { get; set; } = string.Empty;
        public string description { get; set; } = string.Empty;
        public int difficulty { get; set; }
        public string image { get; set; } = string.Empty;
        public string dateCreated { get; set; } = string.Empty;
        public string country { get; set; } = string.Empty;
        public float rating { get; set; }
        public List<string> ingredients { get; set; } = new();
        public List<string> tags { get; set; } = new();
        public List<string> steps { get; set; } = new();
    }
}

