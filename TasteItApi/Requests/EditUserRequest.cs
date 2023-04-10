namespace TasteItApi.Requests
{
    public class EditUserRequest
    {
        public string token { get; set; }
        public string username { get; set; }
        public string imgProfile { get; set; }
        public string biography { get; set; }
    }
}
