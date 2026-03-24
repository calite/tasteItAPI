using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TasteItApi.Graph.Requests;
using TasteItApi.Graph.Services;
using TasteItApi.Requests;

namespace TasteItApi.Controllers
{
    [Authorize]
    [ApiController]
    [Route("recipe")]
    public class RecipeController : ControllerBase
    {
        private readonly IRecipeGraphService _graphService;

        public RecipeController(IRecipeGraphService graphService)
        {
            _graphService = graphService;
        }

        [HttpPost("create")]
        public async Task<IActionResult> PostCreateRecipe([FromBody] RecipeRequest request, CancellationToken cancellationToken)
        {
            var created = await _graphService.CreateRecipeAsync(
                request.name,
                request.description,
                request.token,
                cancellationToken: cancellationToken);

            return Ok(created);
        }

        [HttpPost("{recipeId}/ingredient")]
        public async Task<IActionResult> AddIngredientToRecipe(
            string recipeId,
            [FromBody] AddIngredientToRecipeRequest request,
            CancellationToken cancellationToken)
        {
            var created = await _graphService.AddIngredientToRecipeAsync(
                recipeId,
                request.IngredientName,
                request.IngredientType,
                request.IngredientId,
                cancellationToken);

            return Ok(created);
        }

        [HttpPost("like")]
        public async Task<IActionResult> PostLikeOnRecipe([FromBody] LikeRecipeRequest request, CancellationToken cancellationToken)
        {
            await _graphService.AddLikeToRecipeAsync(request.token, request.rid.ToString(), cancellationToken);
            return Ok();
        }
    }
}
