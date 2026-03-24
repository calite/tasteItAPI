using Microsoft.AspNetCore.Mvc;
using TasteItApi.Graph.Testing.Dtos;
using TasteItApi.Graph.Testing.Services;

namespace TasteItApi.Controllers
{
    [ApiController]
    [Route("test")]
    public class TestGraphController : ControllerBase
    {
        private readonly ITestGraphService _service;

        public TestGraphController(ITestGraphService service)
        {
            _service = service;
        }

        /// <summary>
        /// Seeds the graph with test users, recipes, ingredients, and likes.
        /// </summary>
        [HttpPost("seed")]
        public async Task<IActionResult> Seed(CancellationToken cancellationToken)
        {
            var result = await _service.SeedAsync(cancellationToken);
            return Ok(result);
        }

        /// <summary>
        /// Creates a test user node.
        /// </summary>
        [HttpPost("user")]
        public async Task<IActionResult> CreateUser([FromBody] CreateTestUserRequest request, CancellationToken cancellationToken)
        {
            var userId = await _service.CreateUserAsync(request, cancellationToken);
            return CreatedAtAction(nameof(GetRecipesByUser), new { userId }, new { userId });
        }

        /// <summary>
        /// Creates a test recipe and links it to the creator user.
        /// </summary>
        [HttpPost("recipe")]
        public async Task<IActionResult> CreateRecipe([FromBody] CreateTestRecipeRequest request, CancellationToken cancellationToken)
        {
            var recipeId = await _service.CreateRecipeAsync(request, cancellationToken);
            return CreatedAtAction(nameof(GetIngredients), new { recipeId }, new { recipeId });
        }

        /// <summary>
        /// Adds an ingredient to an existing recipe.
        /// </summary>
        [HttpPost("recipe/{recipeId}/ingredient")]
        public async Task<IActionResult> AddIngredient(string recipeId, [FromBody] AddTestIngredientRequest request, CancellationToken cancellationToken)
        {
            var ingredient = await _service.AddIngredientToRecipeAsync(recipeId, request, cancellationToken);
            return CreatedAtAction(nameof(GetIngredients), new { recipeId }, ingredient);
        }

        /// <summary>
        /// Creates a LIKE relationship between a user and a recipe.
        /// </summary>
        [HttpPost("recipe/{recipeId}/like/{userId}")]
        public async Task<IActionResult> LikeRecipe(string recipeId, string userId, CancellationToken cancellationToken)
        {
            await _service.LikeRecipeAsync(recipeId, userId, cancellationToken);
            return Ok();
        }

        /// <summary>
        /// Returns all recipes in the test graph.
        /// </summary>
        [HttpGet("recipes")]
        public async Task<IActionResult> GetRecipes(CancellationToken cancellationToken)
        {
            var recipes = await _service.GetAllRecipesAsync(cancellationToken);
            return Ok(recipes);
        }

        /// <summary>
        /// Returns recipes filtered by ingredient name.
        /// </summary>
        [HttpGet("recipes/by-ingredient")]
        public async Task<IActionResult> GetRecipesByIngredient([FromQuery] string name, CancellationToken cancellationToken)
        {
            var recipes = await _service.GetRecipesByIngredientAsync(name, cancellationToken);
            return Ok(recipes);
        }

        /// <summary>
        /// Returns recipes created by a specific user.
        /// </summary>
        [HttpGet("recipes/by-user/{userId}")]
        public async Task<IActionResult> GetRecipesByUser(string userId, CancellationToken cancellationToken)
        {
            var recipes = await _service.GetRecipesByUserAsync(userId, cancellationToken);
            return Ok(recipes);
        }

        /// <summary>
        /// Returns ingredients contained in a recipe.
        /// </summary>
        [HttpGet("recipe/{recipeId}/ingredients")]
        public async Task<IActionResult> GetIngredients(string recipeId, CancellationToken cancellationToken)
        {
            var ingredients = await _service.GetIngredientsOfRecipeAsync(recipeId, cancellationToken);
            return Ok(ingredients);
        }

        /// <summary>
        /// Returns collaborative-filtering recipe recommendations for a user.
        /// </summary>
        [HttpGet("recommendations/{userId}")]
        public async Task<IActionResult> GetRecommendations(string userId, CancellationToken cancellationToken)
        {
            var recommendations = await _service.GetRecommendationsAsync(userId, cancellationToken);
            return Ok(recommendations);
        }

        /// <summary>
        /// Returns a debug snapshot of graph nodes and relationships.
        /// </summary>
        [HttpGet("graph")]
        public async Task<IActionResult> GetGraph(CancellationToken cancellationToken)
        {
            var graph = await _service.GetFullGraphAsync(cancellationToken);
            return Ok(graph);
        }
    }
}
