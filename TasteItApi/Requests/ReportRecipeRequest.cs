namespace TasteItApi.Requests
{
    public class ReportRecipeRequest
    {
        public int rid { get; set; }
        public string token { get; set; } = string.Empty;
        public string comment { get; set; } = string.Empty;
    }
}

