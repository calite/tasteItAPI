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

        //DOC: https://github.com/DotNet4Neo4j/Neo4jClient/wiki


        public RecipeController(IGraphClient client)
        {
            _client = client;
        }

        [HttpGet("/recipes")]
        public async Task<ActionResult<Recipe>> GetAllRecipes()
        {
            //devuelve las recetas seguido del usuario que la creo
            var result = await _client.Cypher
                .Match("(user:User)-[:Created]-(recipe:Recipe)")
                .Return((recipe, user) => new
                {
                    Recipe = recipe.As<Recipe>(),
                    User = user.As<User>()
                })
                .ResultsAsync;

            var results = result.ToList();

            if (results.Count == 0)
            {
                return NotFound();
            }

            return Ok(results);
        }

        [HttpGet("/recipe/byname/{name}")]
        public async Task<ActionResult<Recipe>> GetRecipeByName(string name)
        {
            //devuelve recetas filtrando por nombre seguido del usuario que la creo
            var result = await _client.Cypher
                            .Match("(recipe:Recipe)-[:Created]-(user:User)")
                            .Where((Recipe recipe) => recipe.name.Contains(name))
                            .Return((recipe, user) => new
                            {
                                Recipe = recipe.As<Recipe>(),
                                User = user.As<User>()

                            })
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
            //filtramos recetas por ciudad seguido del usuario que la creo
            var result = await _client.Cypher
                            .Match("(recipe:Recipe)-[:Created]-(user:User)")
                            .Where((Recipe r) => r.country.Contains(country))
                            .Return((recipe, user) => new
                            {
                                Recipe = recipe.As<Recipe>(),
                                User = user.As<User>()

                            })
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

        //ARREGLAR
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

        //CREAR RECETA
        [HttpPost("/recipe/create")]
        public async Task<IActionResult> PostCreateRecipe(string token, string name, string description, string country, int difficulty, string ingredients, string steps, string tags)
        {

            //EL TOKEN SE RECOGERA DE LA SESION
            //LA IMAGEN SE DEBE RECOGER DEL CLIENTE

            //token = "xmg10sMQgMS4392zORWGW7TQ1Qg2";

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

        //COMMENTARIO EN LA RECETA
        [HttpPost("/recipe/comment_recipe/{rid}")]
        public async Task<IActionResult> PostCommentRecipe(int rid, string token, string comment, float rating)
        {
            string today = DateTime.Today.ToShortDateString();

            //token = "xmg10sMQgMS4392zORWGW7TQ1Qg2";

            var result = await _client.Cypher
                .Match("(u:User),(r:Recipe)")
                .Where("ID(r) =" + rid)
                .AndWhere("u.token ='" + token + "'")
                .Create("(u) -[c:Commented{ comment:$comment, rating:$rating, dateCreated:$dateCreated}]->(r)")
                .WithParam("comment", comment)
                .WithParam("rating", rating)
                .WithParam("dateCreated", today)
                .Return(r => r.As<Recipe>())
                .ResultsAsync;

            var recipes = result.ToList();

            if (recipes.Count == 0)
            {
                return NotFound();
            }

            return Ok(recipes);
        }

        //REPORTAR UNA RECETA
        [HttpPost("/recipe/report_recipe/{rid}")]
        public async Task<IActionResult> PostReportRecipe(int rid, string token, string comment)
        {
            string today = DateTime.Today.ToShortDateString();

            //token = "xmg10sMQgMS4392zORWGW7TQ1Qg2";

            var result = await _client.Cypher
                .Match("(u:User),(r:Recipe)")
                .Where("ID(r) =" + rid)
                .AndWhere("u.token ='" + token + "'")
                .Create("(u) -[:Reported{ comment:$comment, dateCreated:$dateCreated}]->(r)")
                .WithParam("comment", comment)
                .WithParam("dateCreated", today)
                .Return(r => r.As<Recipe>())
                .ResultsAsync;

            var recipes = result.ToList();

            if (recipes.Count == 0)
            {
                return NotFound();
            }

            return Ok(recipes);
        }

        //CONFIRMAR SI UNA RECETA TIENE ME GUSTA
        [HttpGet("/recipe/isliked")]
        public async Task<IActionResult> GetRecipeIsLiked(int rid, string token)
        {

            //token = "xmg10sMQgMS4392zORWGW7TQ1Qg2";

            var result = await _client.Cypher
                .Match("(u:User)-[c:Liked]->(r:Recipe)")
                .Where("ID(r) = $rid")
                .AndWhere("u.token = $token")
                .WithParam("rid", rid)
                .WithParam("token", token)
                .Return(r => r.As<Recipe>())
                .ResultsAsync;

            var recipes = result.ToList();

            if (recipes.Count == 0)
            {
                return NotFound();
            }

            return Ok();
        }

        //DA O QUITA LIKE A UNA RECETA
        [HttpPost("/recipe/like/{rid}")]
        public async Task<IActionResult> PostLikeRecipe(int rid, string token)
        {
            string today = DateTime.Today.ToShortDateString();

            //token = "xmg10sMQgMS4392zORWGW7TQ1Qg2";

            var result = await _client.Cypher
                .Match("(u:User)-[c:Liked]->(r:Recipe)")
                .Where("ID(r) =" + rid)
                .AndWhere("u.token ='" + token + "'")
                .Return(r => r.As<Recipe>())
                .ResultsAsync;

            var recipes = result.ToList();

            if (recipes.Count != 0) //si no es 0 por lo tanto existe y lo borra
            {
                result = await _client.Cypher
                    .Match("(u:User)-[c:Liked]->(r:Recipe)")
                    .Where("ID(r) =" + rid)
                    .AndWhere("u.token ='" + token + "'")
                    .Delete("c")
                    .Return(r => r.As<Recipe>())
                    .ResultsAsync;

                recipes = result.ToList();
            }
            else
            {
                result = await _client.Cypher
                    .Match("(u:User),(r:Recipe)")
                    .Where("ID(r) =" + rid)
                    .AndWhere("u.token ='" + token + "'")
                    .Create("(u)-[:Liked{dateCreated:'" + today + "'}]->(r)")
                    .Return(r => r.As<Recipe>())
                    .ResultsAsync;

                recipes = result.ToList();
            }

            return Ok(recipes);
        }

        //DEVUELVE LOS USUARIOS QUE HAN DADO LIKE A UNA RECETA
        [HttpGet("/recipe/likes")]
        public async Task<IActionResult> GetRecipeLikes(int rid)
        {

            //token = "xmg10sMQgMS4392zORWGW7TQ1Qg2";

            var result = await _client.Cypher
                .Match("(u:User)-[c:Liked]->(r:Recipe)")
                .Where("ID(r) = $rid")
                .WithParam("rid", rid)
                .Return(u => u.As<User>())
                .ResultsAsync;

            var users = result.ToList();

            if (users.Count == 0)
            {
                return NotFound();
            }

            return Ok(users);
        }

        //DEVUELVE LOS COMENTARIOS DE UNA RECETA
        [HttpGet("/recipe/comments")]
        public async Task<IActionResult> GetCommentsOnRecipe(int rid)
        {

            //token = "xmg10sMQgMS4392zORWGW7TQ1Qg2";

            var result = await _client.Cypher
                .Match("(r:Recipe)-[c:Commented]-(u:User)")
                .Where("ID(r) = $rid")
                .WithParam("rid", rid)
                .Return(c => c.As<Comment>())
                .ResultsAsync;

            var comments = result.ToList();

            if (comments.Count == 0)
            {
                return NotFound();
            }

            return Ok(comments);
        }



    }


}
