namespace TasteItApi.Models
{
    public class Comment
    {
        public string comment { get; set; }
        public DateTimeOffset dateCreated { get; set; }
        public float rating { get; set; }
    }
}
