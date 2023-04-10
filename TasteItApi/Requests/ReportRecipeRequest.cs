namespace TasteItApi.Requests
{
    public class ReportRecipeRequest
    {
        public int rid { get; set; }
        public string token { get; set; }
        public string comment { get; set; }
    }
}
