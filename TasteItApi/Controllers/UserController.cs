using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TasteItApi.Graph.Services;
using TasteItApi.Models;

namespace TasteItApi.Controllers
{
    [Authorize]
    [ApiController]
    [Route("user")]
    public class UserController : ControllerBase
    {
        private readonly IRecipeGraphService _graphService;

        public UserController(IRecipeGraphService graphService)
        {
            _graphService = graphService;
        }

        [AllowAnonymous]
        [HttpPost("register")]
        public async Task<IActionResult> PostRegisterUser([FromBody] User user, CancellationToken cancellationToken)
        {
            var created = await _graphService.CreateUserAsync(
                user.token,
                user.username,
                string.Empty,
                cancellationToken);

            return Ok(created);
        }
    }
}
