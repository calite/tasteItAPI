namespace TasteItApi.Requests
{
    public class FollowUserRequest
    {
        public string senderToken { get; set; } = string.Empty;
        public string receiverToken { get; set; } = string.Empty;
    }
}

