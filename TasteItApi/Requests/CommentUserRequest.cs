namespace TasteItApi.Requests
{
    public class CommentUserRequest
    {
        public string SenderId { get; set; } = string.Empty;
        public string ReceiverId { get; set; } = string.Empty;
        public string Comment { get; set; } = string.Empty;
    }
}

