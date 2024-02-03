using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Neo4j.Driver;
using Neo4jClient;
using Neo4jClient.Extensions;
using TasteItApi.authentication;
using TasteItApi.Models;
using TasteItApi.Requests;

namespace TasteItApi.Controllers
{
    [Authorize]
    //[AuthByProfile(new string[] { "101" })]
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
        [HttpGet("/admin/recipes/all")]
        public async Task<ActionResult<List<object>>> GetRecipesReported()
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
                    .OrderBy("count(report) desc")
                .ResultsAsync;

                var results = query.ToList();

                return Ok(results.ToList());

            }
            catch(Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        //filtro
        [HttpGet("/admin/recipes/filter")]
        public async Task<ActionResult<RecipeId_Recipe_User>> GetRecipesReportedFiltered(string? name, string? creator, bool? active)
        {
            try
            {
                var result = await _client.Cypher
                .Match("(recipe:Recipe)-[:Created]-(u1:User)")
                .Where("($name IS NULL OR toLower(recipe.name) CONTAINS toLower($name))")
                .AndWhere("($creator IS NULL OR toLower(u1.username) CONTAINS toLower($creator))")
                .AndWhere("($active IS NULL OR recipe.active = $active)")
                .OptionalMatch("(recipe)-[report:Reported]-(u2:User)")
                .WithParam("name", name)
                .WithParam("creator", creator)
                .WithParam("active", active)
                 .Return((recipe, u1, report) => new
                 {
                     recipeId = recipe.Id(),
                     recipe = recipe.As<RecipeWEB>(),
                     creator = u1.As<User>(),
                     reportsCount = report.Count()
                 })
                .OrderBy("count(report) desc")
                .ResultsAsync;

                var recipes = result.ToList();

                return Ok(recipes);

            } catch(Exception ex)
            {
                return BadRequest(ex);
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

                return Ok(results); 
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

                return Ok(results);
            }
            catch (Exception ex)
            {
                return BadRequest(ex);
            }
        }

        //devuelve los reports de una receta
        [HttpGet("/admin/reports-recipe/{id:int}")]
        public async Task<ActionResult> GetReportsOnRecipe(int id)
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

                return Ok(results);
            }
            catch (Exception ex)
            {
                return BadRequest();
            }
        }

    }
}
