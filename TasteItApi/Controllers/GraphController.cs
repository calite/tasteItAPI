using Microsoft.AspNetCore.Mvc;
using TasteItApi.Graph.Requests;
using TasteItApi.Graph.Services;

namespace TasteItApi.Controllers
{
    [ApiController]
    [Route("graph")]
    public class GraphController : ControllerBase
    {
        private readonly IRecipeGraphService _graphService;

        public GraphController(IRecipeGraphService graphService)
        {
            _graphService = graphService;
        }

        [HttpPost("users")]
        public async Task<IActionResult> CreateUser([FromBody] CreateUserNodeRequest request, CancellationToken cancellationToken)
        {
            var created = await _graphService.CreateUserAsync(request.Id, request.Name, request.Email, cancellationToken);
            return Ok(created);
        }

        [HttpPost("recipes")]
        public async Task<IActionResult> CreateRecipe([FromBody] CreateRecipeNodeRequest request, CancellationToken cancellationToken)
        {
            var created = await _graphService.CreateRecipeAsync(
                request.Id,
                request.Name,
                request.Description,
                request.CreatedByUserId,
                cancellationToken);

            return Ok(created);
        }

        [HttpPost("recipes/ingredients")]
        public async Task<IActionResult> AddIngredientToRecipe([FromBody] AddIngredientToRecipeRequest request, CancellationToken cancellationToken)
        {
            var ingredient = await _graphService.AddIngredientToRecipeAsync(
                request.RecipeId,
                request.IngredientId,
                request.IngredientName,
                request.IngredientType,
                cancellationToken);

            return Ok(ingredient);
        }

        [HttpPost("users/{userId}/likes/{recipeId}")]
        public async Task<IActionResult> AddLikeToRecipe(string userId, string recipeId, CancellationToken cancellationToken)
        {
            await _graphService.AddLikeToRecipeAsync(userId, recipeId, cancellationToken);
            return Ok();
        }
    }
}
