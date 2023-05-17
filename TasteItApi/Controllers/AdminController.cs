using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Neo4jClient;
using TasteItApi.Models;

namespace TasteItApi.Controllers
{
    public class AdminController : ControllerBase
    {
        private readonly IGraphClient _client;

        public AdminController(IGraphClient client)
        {
            _client = client;
        }

        [HttpGet("admin/recipes/all/{skipper:int}")]
        public async Task<ActionResult<Recipe>> GetAllRecipesWithSkipper(int skipper)
        {
            //devuelve las recetas seguido del usuario que la creo
            var result = await _client.Cypher
                .Match("(recipe:Recipe)-[:Created]-(user:User)")
                .Return((recipe, user) => new
                {
                    RecipeId = recipe.Id(),
                    Recipe = recipe.As<Recipe>(),
                    User = user.As<User>()
                })
                .OrderBy("recipe.dateCreated desc")
                .Skip(skipper)
                .Limit(10)
                .ResultsAsync;

            var results = result.ToList();

            return Ok(results);
        }

        [HttpGet("/admin/recipe/{id:int}")]
        public async Task<ActionResult<Recipe>> GetRecipeById(int id)
        {
            //devuelve las recetas seguido del usuario que la creo
            var result = await _client.Cypher
                .Match("(user:User)-[:Created]-(recipe:Recipe)")
                .Where("ID(recipe) = " + id)
                .Return((recipe, user) => new
                {
                    RecipeId = recipe.Id(),
                    Recipe = recipe.As<Recipe>(),
                    User = user.As<User>()
                })
                .ResultsAsync;

            //var results = result.ToList();

            return Ok(result);
        }



    }
}
