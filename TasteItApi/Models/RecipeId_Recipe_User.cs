namespace TasteItApi.Models
{
    public class RecipeId_Recipe_User
    {
        public long RecipeId { get; set; }
        public Recipe Recipe { get; set; }
        public  User User { get; set; }
    }
}
