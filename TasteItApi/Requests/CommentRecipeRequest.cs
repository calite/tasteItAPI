namespace TasteItApi.Requests
{
    public class CommentRecipeRequest
    {
        public int rid { get; set; }
        public string token { get; set; }
        public string comment { get; set; }
        public float rating { get; set; }
    }
}
