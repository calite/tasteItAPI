using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TasteItApi.Graph.Admin.Services;
using TasteItApi.Models;
using TasteItApi.Requests;

namespace TasteItApi.Controllers
{
    [Authorize]
    [ApiController]
    [Route("admin")]
    public class AdminController : ControllerBase
    {
        private readonly IAdminGraphService _adminGraphService;

        public AdminController(IAdminGraphService adminGraphService)
        {
            _adminGraphService = adminGraphService;
        }

        /// <summary>
        /// Returns all recipes with report counters for moderation.
        /// </summary>
        [HttpGet("recipes/all")]
        public async Task<ActionResult<List<object>>> GetRecipesReported(CancellationToken cancellationToken)
        {
            var rows = await _adminGraphService.GetRecipesReportedAsync(cancellationToken);

            return Ok(rows);
        }

        /// <summary>
        /// Returns reported recipes filtered by name, creator, and active state.
        /// </summary>
        [HttpGet("recipes/filter")]
        public async Task<ActionResult> GetRecipesReportedFiltered(string? name, string? creator, bool? active, CancellationToken cancellationToken)
        {
            var rows = await _adminGraphService.GetRecipesReportedFilteredAsync(name, creator, active, cancellationToken);

            return Ok(rows);
        }

        /// <summary>
        /// Returns one recipe with creator details by internal Neo4j id.
        /// </summary>
        [HttpGet("recipe/{id:int}")]
        public async Task<ActionResult> GetRecipeReportedById(int id, CancellationToken cancellationToken)
        {
            var rows = await _adminGraphService.GetRecipeReportedByIdAsync(id, cancellationToken);

            return Ok(rows);
        }

        /// <summary>
        /// Changes active state of a recipe for moderation.
        /// </summary>
        [HttpPost("recipe/change_state")]
        public async Task<IActionResult> PostChangeStateRecipe([FromBody] ChangeStateRecipeRequest request, CancellationToken cancellationToken)
        {
            var rows = await _adminGraphService.ChangeRecipeStateAsync(request.rid, request.value, cancellationToken);

            return Ok(rows);
        }

        /// <summary>
        /// Returns reports attached to a recipe by internal Neo4j id.
        /// </summary>
        [HttpGet("reports-recipe/{id:int}")]
        public async Task<ActionResult> GetReportsOnRecipe(int id, CancellationToken cancellationToken)
        {
            var rows = await _adminGraphService.GetReportsOnRecipeAsync(id, cancellationToken);

            return Ok(rows);
        }
    }
}
