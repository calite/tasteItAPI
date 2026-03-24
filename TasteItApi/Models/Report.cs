namespace TasteItApi.Models
{
    public class Report
    {
        public string comment { get; set; } = string.Empty;
        public DateTimeOffset dateCreated { get; set; }
    }
}

