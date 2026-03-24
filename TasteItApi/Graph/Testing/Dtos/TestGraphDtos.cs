using System.ComponentModel.DataAnnotations;

namespace TasteItApi.Graph.Testing.Dtos
{
    public class CreateTestUserRequest
    {
        [Required]
        [MaxLength(120)]
        public string Name { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        [MaxLength(256)]
        public string Email { get; set; } = string.Empty;
    }

    public class CreateTestRecipeRequest
    {
        [Required]
        [MaxLength(180)]
        public string Name { get; set; } = string.Empty;

        [Required]
        [MaxLength(2000)]
        public string Description { get; set; } = string.Empty;

        [Required]
        [MaxLength(128)]
        public string UserId { get; set; } = string.Empty;
    }

    public class AddTestIngredientRequest
    {
        [Required]
        [MaxLength(100)]
        public string IngredientName { get; set; } = string.Empty;

        [Required]
        [MaxLength(50)]
        public string Type { get; set; } = string.Empty;
    }

    public class SeedResultDto
    {
        public int Users { get; set; }
        public int Recipes { get; set; }
        public int Ingredients { get; set; }
        public int Likes { get; set; }
    }

    public class TestRecipeDto
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public string CreatedByUserId { get; set; } = string.Empty;
    }

    public class TestIngredientDto
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public double Quantity { get; set; }
        public string Unit { get; set; } = string.Empty;
    }

    public class TestRecommendationDto
    {
        public string RecipeId { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public int Score { get; set; }
    }

    public class GraphNodeDto
    {
        public string ElementId { get; set; } = string.Empty;
        public IReadOnlyList<string> Labels { get; set; } = Array.Empty<string>();
        public IDictionary<string, object?> Properties { get; set; } = new Dictionary<string, object?>();
    }

    public class GraphRelationshipDto
    {
        public string ElementId { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public string StartNodeElementId { get; set; } = string.Empty;
        public string EndNodeElementId { get; set; } = string.Empty;
        public IDictionary<string, object?> Properties { get; set; } = new Dictionary<string, object?>();
    }

    public class GraphSnapshotDto
    {
        public IReadOnlyList<GraphNodeDto> Nodes { get; set; } = Array.Empty<GraphNodeDto>();
        public IReadOnlyList<GraphRelationshipDto> Relationships { get; set; } = Array.Empty<GraphRelationshipDto>();
    }
}
