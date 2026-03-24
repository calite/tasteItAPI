namespace TasteItApi.Requests
{
    public class EditUserRequest
    {
        public string token { get; set; } = string.Empty;
        public string username { get; set; } = string.Empty;
        public string imgProfile { get; set; } = string.Empty;
        public string biography { get; set; } = string.Empty;
    }
}

