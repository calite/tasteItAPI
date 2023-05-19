using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Neo4jClient;
using Neo4jClient.Extensions;
using TasteItApi.Models;
using TasteItApi.Requests;

namespace TasteItApi.Controllers
{
    [Authorize]
    [ApiController]
    [Route("controller")]
    public class AdminController : Controller
    {
        private readonly IGraphClient _client;

        public AdminController(IGraphClient client)
        {
            _client = client;
        }

        //devuelve todas las recetas seguido del creador y el numero de reports que tienen
        [HttpGet("/admin/recipes/all/{skipper:int}")]
        public async Task<ActionResult<List<object>>> GetRecipesReported(int skipper)
        {
            try
            {
                var query = await _client.Cypher
                    .Match("(recipe:Recipe)-[:Created]-(u1:User)")
                    .OptionalMatch("(recipe)-[report:Reported]-(u2:User)")
                    .Return((recipe, u1, report) => new
                    {
                        recipeId = recipe.Id(),
                        recipe = recipe.As<RecipeWEB>(),
                        creator = u1.As<User>(),
                        reportsCount = report.Count()
                    })
                    .OrderByDescending("recipe.dateCreated")
                    .Skip(skipper)
                    .Limit(20)
                .ResultsAsync;

                var results = query.ToList();

                if(results.Count > 0)
                {
                    return Ok(results.ToList());
                }else
                {
                    return NotFound();
                }
            }
            catch(Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        //devuelve una receta especifica a partir de un ID
        [HttpGet("/admin/recipe/{id:int}")]
        public async Task<ActionResult<Recipe>> GetRecipeReportedById(int id)
        {
            try
            {
                var query = await _client.Cypher
                    .Match("(user:User)-[:Created]-(recipe:Recipe)")
                    .Where("ID(recipe) = " + id)
                    .Return((recipe, user) => new
                    {
                        RecipeId = recipe.Id(),
                        Recipe = recipe.As<RecipeWEB>(),
                        User = user.As<User>()
                    })
                .ResultsAsync;

                var results = query.ToList();

                if(results.Count > 0)
                {
                    return Ok(results);
                } else
                {
                    return NotFound();
                } 
            }
            catch(Exception ex)
            {
                return BadRequest(ex);
            }         

        }

        //destinado a cambiar el estado de una receta
        [HttpPost("/admin/recipe/change_state")]
        public async Task<IActionResult> PostChangeStateRecipe([FromBody] ChangeStateRecipeRequest request)
        {
            try
            {
                var query = await _client.Cypher
                    .Match("(recipe:Recipe)")
                    .Where("Id(recipe) = $rid")
                    .Set("recipe.active = $value")
                    .WithParam("rid", request.rid)
                    .WithParam("value", request.value)
                    .Return(recipe => recipe.As<RecipeWEB>())
                .ResultsAsync;

                var results = query.ToList();

                if(results.Count > 0)
                {
                    return Ok(results);
                } else
                {
                    return NotFound();
                }
            }
            catch (Exception ex)
            {
                return BadRequest(ex);
            }
        }

        [HttpGet("/admin/reports-recipe/{id:int}")]
        public async Task<ActionResult<Recipe>> GetReportsOnRecipe(int id)
        {
            try
            {
                var query = await _client.Cypher
                    .Match("(user:User)-[report:Reported]-(recipe:Recipe)")
                    .Where("ID(recipe) = " + id)
                    .Return((user, report) => new
                    {
                        User = user.As<User>(),
                        Report = report.As<Report>()
                    })
                .ResultsAsync;

                var results = query.ToList();

                if(results.Count > 0)
                {
                    return Ok(results);
                } else
                {
                    return NotFound();
                }
            }
            catch (Exception ex)
            {
                return BadRequest();
            }
        }

    }
}
