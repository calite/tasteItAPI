namespace TasteItApi.Models
{
    public class RecipeId_RecipeWEB_User
    {
        public long RecipeId { get; set; }
        public RecipeWEB Recipe { get; set; }
        public  User User { get; set; }
    }
}
