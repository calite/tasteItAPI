namespace TasteItApi.Requests
{
    public class EditRecipeRequest
    {
        public int rid { get; set; }
        public string name { get; set; }
        public string description { get; set; }
        public string country { get; set; }
        public string image { get; set; }
        public int difficulty { get; set; }
        public string ingredients { get; set; }
        public string steps { get; set; }
        public string tags { get; set; }
    }
}
