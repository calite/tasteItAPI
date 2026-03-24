namespace TasteItApi.Requests
{
    public class CommentRecipeRequest
    {
        public int rid { get; set; }
        public string token { get; set; } = string.Empty;
        public string comment { get; set; } = string.Empty;
        public float rating { get; set; }
    }
}

