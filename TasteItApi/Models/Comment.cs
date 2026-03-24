namespace TasteItApi.Models
{
    public class Comment
    {
        public string comment { get; set; } = string.Empty;
        public DateTimeOffset dateCreated { get; set; }
        public float rating { get; set; }
    }
}

