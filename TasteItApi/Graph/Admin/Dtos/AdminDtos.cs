using TasteItApi.Models;

namespace TasteItApi.Graph.Admin.Dtos
{
    public class AdminReportedRecipeDto
    {
        public long recipeId { get; set; }
        public RecipeWEB recipe { get; set; } = new();
        public User creator { get; set; } = new();
        public long reportsCount { get; set; }
    }

    public class AdminRecipeByIdDto
    {
        public long RecipeId { get; set; }
        public RecipeWEB Recipe { get; set; } = new();
        public User User { get; set; } = new();
    }

    public class AdminReportDto
    {
        public User User { get; set; } = new();
        public Report Report { get; set; } = new();
    }
}
