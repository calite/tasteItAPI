namespace TasteItApi.Requests
{
    public class RecipeRequest
    {
        public string token { get; set; }
        public string name { get; set; }
        public string description { get; set; }
        public string country { get; set; }
        public string image { get; set; }
        public int difficulty { get; set; }
        public string[] ingredients { get; set; }
        public string[] steps { get; set; }

    }
}
