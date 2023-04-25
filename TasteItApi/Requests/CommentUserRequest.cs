namespace TasteItApi.Requests
{
    public class CommentUserRequest
    {
        public string SenderId { get; set; }
        public string ReceiverId { get; set; }
        public string Comment { get; set; }
    }
}
