using Microsoft.AspNetCore.Mvc;
using Neo4j.Driver;
using Neo4jClient;
using Neo4jClient.Cypher;
using Newtonsoft.Json.Linq;
using System;
using System.Linq;
using System.Net.WebSockets;
using TasteItApi.Models;
using static System.Net.Mime.MediaTypeNames;

namespace TasteItApi.Controllers
{
    [ApiController]
    [Route("controller")]
    public class RecipeController : Controller
    {
        private readonly IGraphClient _client;
        private IRawGraphClient _rawClient;

        public RecipeController(IGraphClient client)
        {
            _client = client;
        }

        [HttpGet("/recipes")]
        public async Task<ActionResult<User>> GetAllRecipes()
        {

            //"MATCH (n1:User)-[:Created]-(n2:Recipe) RETURN n2.name, n2.description, n2.steps, n2.image, n2.dateCreated, n2.country, n1.username, n2.difficulty, n2.tags, n2.ingredients, ID(n2);;
            var result = await _client.Cypher
                            .Match("(r:Recipe)")
                            .Return(r => r.As<Recipe>())
                            .ResultsAsync;

            var recipes = result.ToList();

            if (recipes.Count == 0)
            {
                return NotFound();
            }

            return Ok(recipes);
        }

        [HttpGet("/recipe/byname/{name}")]
        public async Task<ActionResult<Recipe>> GetRecipeByName(string name)
        {
            var result = await _client.Cypher
                            .Match("(r:Recipe)")
                            .Where((Recipe r) => r.name.Contains(name))
                            .Return(r => r.As<Recipe>())
                            .ResultsAsync;

            var recipe = result.ToList();

            if (recipe == null)
            {
                return NotFound();
            }

            return Ok(recipe);
        }

        [HttpGet("/recipe/bycountry/{country}")]
        public async Task<ActionResult<Recipe>> GetRecipeByCountry(string country)
        {
            var result = await _client.Cypher
                            .Match("(r:Recipe)")
                            .Where((Recipe r) => r.country.Contains(country))
                            .Return(r => r.As<Recipe>())
                            .ResultsAsync;

            var recipe = result.ToList();

            if (recipe == null)
            {
                return NotFound();
            }

            return Ok(recipe);
        }

        [HttpGet("/recipe/byuser/{token}")]
        public async Task<ActionResult<User>> GetRecipesByUser(string token)
        {

            //MATCH (n1:User)-[:Created]-(n2:Recipe) WHERE n1.token = '" + uid + "' RETURN n1.username, n2;
            var result = await _client.Cypher
                            .Match("(u:User)-[:Created]-(r:Recipe)")
                            .Where((User u) => u.token == token)
                            .Return(r => r.As<Recipe>())
                            .ResultsAsync;

            var creators = result.ToList();

            if (creators.Count == 0)
            {
                return NotFound();
            }

            return Ok(creators);
        }

        //no furula
        [HttpGet("/recipe/byingredients/{ingredients}")]
        public async Task<ActionResult<List<Recipe>>> GetRecipeByIngredients(string ingredients)
        {

            List<string> listIng = ingredients.Split(",").ToList();

            var result = await _client.Cypher
                            .Match("(r:Recipe)")
                            .Where("$ingredient IN r.ingredients")
                            .WithParam("ingredient", listIng)
                            .Return(r => r.As<Recipe>())
                            .ResultsAsync;

            var recipes = result.ToList();

            if (recipes.Count == 0)
            {
                return NotFound();
            }

            return Ok(recipes);
        }


        [HttpPost("/create")]
        public async Task<IActionResult> CreateRecipe(string token, string name, string description, string country, int difficulty, string ingredients, string steps, string tags )
        {

            //EL TOKEN SE RECOGERA DE LA SESION
            //LA IMAGEN SE DEBE RECOGER DEL CLIENTE

            token = "xmg10sMQgMS4392zORWGW7TQ1Qg2";

            string today = DateTime.Today.ToShortDateString();

            List<string> listIng = ingredients.Split(",").ToList();
            List<string> listSteps = steps.Split(",").ToList();
            List<string> listTags = tags.Split(",").ToList();

            var result = await _client.Cypher
                            .Match("(u: User)")
                            .Where((User u) => u.token == token)
                            .Create("(u)-[:Created]->(r:Recipe {name:$name,description:$description,country:$country,dateCreated:$dateCreated,image:$image,difficulty:$difficulty,steps:$steps,ingredients:$ingredients,tags:$tags})")
                            .WithParam("name", name)
                            .WithParam("description", description)
                            .WithParam("country", country)
                            .WithParam("dateCreated", today)
                            .WithParam("image", "")
                            .WithParam("difficulty", difficulty)
                            .WithParam("steps", listSteps)
                            .WithParam("ingredients", listIng)
                            .WithParam("tags", listTags)
                            .Return(r => r.As<Recipe>())
                            .ResultsAsync;

            return Ok(result);
        }

    }
}
