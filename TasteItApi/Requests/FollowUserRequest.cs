namespace TasteItApi.Requests
{
    public class FollowUserRequest
    {
        public string senderToken { get; set; }
        public string receiverToken { get; set; }
    }
}
