using Microsoft.AspNetCore.Mvc;
using Neo4j.Driver;
using Neo4jClient;
using Neo4jClient.Cypher;
using Newtonsoft.Json.Linq;
using System;
using System.Linq;
using System.Net.WebSockets;
using TasteItApi.Models;
using TasteItApi.Requests;
using static System.Net.Mime.MediaTypeNames;

namespace TasteItApi.Controllers
{
    [ApiController]
    [Route("controller")]
    public class RecipeController : Controller
    {
        private readonly IGraphClient _client;

        //DOC: https://github.com/DotNet4Neo4j/Neo4jClient/wiki


        public RecipeController(IGraphClient client)
        {
            _client = client;
        }

        [HttpGet("/recipe/all")]
        public async Task<ActionResult<Recipe>> GetAllRecipes()
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
                .ResultsAsync;

            var results = result.ToList();

            if (results.Count == 0)
            {
                return NotFound();
            }

            return Ok(results);
        }

        [HttpGet("/recipe/all/{skipper:int}")]
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

            if (results.Count == 0)
            {
                return NotFound();
            }

            return Ok(results);
        }

        [HttpGet("/recipe/{id:int}")]
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

            var results = result.ToList();

            if (results.Count == 0)
            {
                return NotFound();
            }

            return Ok(results);
        }

        [HttpGet("/recipe/random/{limit}")]
        public async Task<ActionResult<Recipe>> GetRandomRecipesWithLimit(int limit)
        {

            /*
                match (u:User)-[c:Created]-(r:Recipe)
                with u, rand() as rand
                order by rand limit 5
                match (u:User)-[c:Created]-(r:Recipe)
                return r ,u , c , rand() as rand 
            */

            //devuelve un numero aleatorio de recetas seguido del usuario que la creo
            var result = await _client.Cypher
                .Match("(user:User)-[:Created]-(recipe:Recipe)")
                .With("recipe, rand() as rand")
                .OrderBy("rand limit $limit")
                .Match("(user:User)-[:Created]-(recipe:Recipe)")
                .WithParam("limit", limit)
                .Return((recipe, user) => new
                {
                    RecipeId = recipe.Id(),
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
                                RecipeId = recipe.Id(),
                                Recipe = recipe.As<Recipe>(),
                                User = user.As<User>()

                            })
                            .ResultsAsync;

            var recipe = result.ToList();

            if (recipe.Count == 0)
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
                            .Where((Recipe recipe) => recipe.country.Contains(country))
                            .Return((recipe, user) => new
                            {
                                RecipeId = recipe.Id(),
                                Recipe = recipe.As<Recipe>(),
                                User = user.As<User>()

                            })
                            .ResultsAsync;

            var recipe = result.ToList();

            if (recipe.Count == 0)
            {
                return NotFound();
            }

            return Ok(recipe);
        }

        [HttpGet("/recipe/byuser/{token}")]
        public async Task<ActionResult<User>> GetRecipesByUser(string token)
        {

            //token = "xmg10sMQgMS4392zORWGW7TQ1Qg2";

            //MATCH (n1:User)-[:Created]-(n2:Recipe) WHERE n1.token = '" + uid + "' RETURN n1.username, n2;
            var result = await _client.Cypher
                            .Match("(user:User)-[:Created]-(recipe:Recipe)")
                            .Where((User user) => user.token == token)
                             .Return((recipe, user) => new
                             {
                                 RecipeId = recipe.Id(),
                                 Recipe = recipe.As<Recipe>(),
                                 User = user.As<User>()
                             })
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

            List<string> listIng = ingredients.Replace(" ", "").Split(",").ToList();

            var result = await _client.Cypher
                            .Match("(recipe:Recipe)-[:Created]-(user:User)")
                            .Return((recipe, user) => new
                            {
                                RecipeId = recipe.Id(),
                                Recipe = recipe.As<Recipe>(),
                                User = user.As<User>()
                            })
                            .ResultsAsync;

            var recipes = result.ToList();

            if (recipes.Count == 0)
            {
                return NotFound();
            }

            Dictionary<Recipe, User> listRecipesFiltered = new Dictionary<Recipe, User>();

            for (int i = 0; i < recipes.Count; i++)
            {
                //miramos si la lista de los ingredientes contiene alguno de los elementos de los introducidos por el usuario
                bool hasMatch = recipes[i].Recipe.ingredients.Any(x => listIng.Any(y => y == x));

                if (hasMatch)
                {
                    listRecipesFiltered.Add(recipes[i].Recipe, recipes[i].User);
                }

            }

            return Ok(listRecipesFiltered.ToList());
        }

        //CREAR RECETA
        [HttpPost("/recipe/create")]
        //public async Task<IActionResult> PostCreateRecipe(string token, string name, string description, string country, string image, int difficulty, string ingredients, string steps, string tags)
        public async Task<IActionResult> PostCreateRecipe([FromBody] RecipeRequest recipeRequest)
        {
            try
            {
                //EL TOKEN SE RECOGERA DE LA SESION
                //LA IMAGEN SE DEBE RECOGER DEL CLIENTE

                //token = "xmg10sMQgMS4392zORWGW7TQ1Qg2";

                string today = DateTime.Today.ToShortDateString();

                List<string> listIng = recipeRequest.ingredients.Split(",").ToList();
                List<string> listSteps = recipeRequest.steps.Split(",").ToList();
                List<string> listTags = recipeRequest.tags.Split(",").ToList();

                //NOTA: hay que autogenerar los tags

                await _client.Cypher
                    .Match("(user: User)")
                    .Where((User user) => user.token == recipeRequest.token)
                    .Create("(recipe:Recipe {name:$name,description:$description,country:$country,dateCreated:$dateCreated,image:$image,difficulty:$difficulty,steps:$steps,ingredients:$ingredients,tags:$tags})-[c:Created]->(user)")
                    .WithParam("name", recipeRequest.name)
                    .WithParam("description", recipeRequest.description)
                    .WithParam("country", recipeRequest.country)
                    .WithParam("dateCreated", today)
                    .WithParam("image", recipeRequest.image)
                    .WithParam("difficulty", recipeRequest.difficulty)
                    .WithParam("steps", listSteps)
                    .WithParam("ingredients", listIng)
                    .WithParam("tags", listTags)
                    .ExecuteWithoutResultsAsync();

                return Ok();

                // No se devuelve nada explícitamente, ya que se usa "async Task" como tipo de retorno
            }
            catch (Exception ex)
            {
                return BadRequest();
            }

        }

        //COMMENTARIO EN LA RECETA
        [HttpPost("/recipe/comment_recipe")]
        public async Task<IActionResult> PostCommentRecipe([FromBody] CommentRequest commentRequest)
        {
            string today = DateTime.Today.ToShortDateString();

            //token = "xmg10sMQgMS4392zORWGW7TQ1Qg2";

            var result = await _client.Cypher
                .Match("(user:User),(recipe:Recipe)")
                .Where("ID(recipe) =" + commentRequest.rid)
                .AndWhere("user.token ='" + commentRequest.token + "'")
                .Create("(user)-[cmt:Commented{ comment:$comment, rating:$rating, dateCreated:$dateCreated}]->(recipe)")
                .WithParam("comment", commentRequest.comment)
                .WithParam("rating", commentRequest.rating)
                .WithParam("dateCreated", today)
                .Return((recipe, user, cmt) => new
                {
                    RecipeId = recipe.Id(),
                    Recipe = recipe.As<Recipe>(),
                    User = user.As<User>(),
                    cmt = cmt.As<Comment>()
                })
                .ResultsAsync;

            var recipe = result.ToList();

            if (recipe.Count == 0)
            {
                return NotFound();
            }

            return Ok(recipe);
        }

        //REPORTAR UNA RECETA
        [HttpPost("/recipe/report_recipe")]
        public async Task<IActionResult> PostReportRecipe([FromBody] ReportRecipeRequest reportRecipeRequest)
        {
            string today = DateTime.Today.ToShortDateString();

            //token = "xmg10sMQgMS4392zORWGW7TQ1Qg2";

            var result = await _client.Cypher
                .Match("(user:User),(recipe:Recipe)")
                .Where("ID(recipe) =" + reportRecipeRequest.rid)
                .AndWhere("user.token ='" + reportRecipeRequest.token + "'")
                .Create("(user)-[:Reported{comment:$comment,dateCreated:$dateCreated}]->(recipe)")
                .WithParam("comment", reportRecipeRequest.comment)
                .WithParam("dateCreated", today)
                .Return((recipe, user) => new
                {
                    RecipeId = recipe.Id(),
                    Recipe = recipe.As<Recipe>(),
                    User = user.As<User>()
                })
                .ResultsAsync;

            var recipes = result.ToList();

            if (recipes.Count == 0)
            {
                return NotFound();
            }

            return Ok(recipes);
        }

        //CONFIRMAR SI UNA RECETA TIENE ME GUSTA
        [HttpGet("/recipe/isliked/{rid}_{token}")]
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

            var recipe = result.ToList();

            if (recipe.Count == 0)
            {
                return NotFound();
            }

            return Ok(recipe);
        }

        //DA O QUITA LIKE A UNA RECETA
        [HttpPost("/recipe/like/{rid}_{token}")]
        public async Task<IActionResult> PostLikeOnRecipe(LikeRecipeRequest likeRecipeRequest)
        {
            string today = DateTime.Today.ToShortDateString();

            //token = "xmg10sMQgMS4392zORWGW7TQ1Qg2";

            var result = await _client.Cypher
                .Match("(u:User)-[c:Liked]->(r:Recipe)")
                .Where("ID(r) =" + likeRecipeRequest.rid)
                .AndWhere("u.token ='" + likeRecipeRequest.token + "'")
                .Return(r => r.As<Recipe>())
                .ResultsAsync;

            var recipes = result.ToList();

            if (recipes.Count != 0) //si no es 0 por lo tanto existe y lo borra
            {
                result = await _client.Cypher
                    .Match("(u:User)-[c:Liked]->(r:Recipe)")
                    .Where("ID(r) =" + likeRecipeRequest.rid)
                    .AndWhere("u.token ='" + likeRecipeRequest.token + "'")
                    .Delete("c")
                    .Return(r => r.As<Recipe>())
                    .ResultsAsync;

                recipes = result.ToList();
            }
            else
            {
                result = await _client.Cypher
                    .Match("(u:User),(r:Recipe)")
                    .Where("ID(r) =" + likeRecipeRequest.rid)
                    .AndWhere("u.token ='" + likeRecipeRequest.token + "'")
                    .Create("(u)-[:Liked{dateCreated:'" + today + "'}]->(r)")
                    .Return(r => r.As<Recipe>())
                    .ResultsAsync;

                recipes = result.ToList();
            }

            return Ok(recipes);
        }

        //DEVUELVE LOS USUARIOS QUE HAN DADO LIKE A UNA RECETA
        [HttpGet("/recipe/likes/{rid}")]
        public async Task<IActionResult> GetLikesOnRecipe(int rid)
        {

            //token = "xmg10sMQgMS4392zORWGW7TQ1Qg2";

            var result = await _client.Cypher
                .Match("(u:User)-[c:Liked]->(r:Recipe)")
                .Where("ID(r) = $rid")
                .WithParam("rid", rid)
                .Return((r, u) => new
                {
                    RecipeId = r.Id(),
                    Recipe = r.As<Recipe>(),
                    User = u.As<User>()
                })
                .ResultsAsync;

            var users = result.ToList();

            if (users.Count == 0)
            {
                return NotFound();
            }

            return Ok(users);
        }

        //DEVUELVE LOS COMENTARIOS DE UNA RECETA
        [HttpGet("/recipe/comments/{rid}")]
        public async Task<IActionResult> GetCommentsOnRecipe(int rid)
        {

            //token = "xmg10sMQgMS4392zORWGW7TQ1Qg2";

            var result = await _client.Cypher
                .Match("(recipe:Recipe)-[c:Commented]-(u:User)")
                .Where("ID(recipe) = $rid")
                .WithParam("rid", rid)
                .Return((recipe, c, u) => new
                {
                    RecipeId = recipe.Id(),
                    Recipe = recipe.As<Recipe>(),
                    User = u.As<User>(),
                    c = c.As<User>()

                })
                .ResultsAsync;

            var comments = result.ToList();

            if (comments.Count == 0)
            {
                return NotFound();
            }

            return Ok(comments);
        }


        //UPDATES

        //CREAR RECETA
        [HttpPost("/recipe/edit")]
        public async Task<IActionResult> PostEditRecipe([FromBody] EditRecipeRequest editRecipeRequest)
        {
            try
            {

                List<string> listIng = editRecipeRequest.ingredients.Split(",", StringSplitOptions.RemoveEmptyEntries).Select(s => s.Trim()).ToList();
                List<string> listSteps = editRecipeRequest.steps.Split(",", StringSplitOptions.RemoveEmptyEntries).Select(s => s.Trim()).ToList();
                List<string> listTags = editRecipeRequest.tags.Split(",", StringSplitOptions.RemoveEmptyEntries).Select(s => s.Trim()).ToList();


                //NOTA: hay que autogenerar los tags

                await _client.Cypher
                    .Match("(r:Recipe)")
                    .Where("Id(r)=" + editRecipeRequest.rid)
                    .Set("r.name =$name,r.description=$description,r.country =$country,r.image=$image,r.difficulty=$difficulty,r.steps=$listSteps,r.ingredients=$listIng,r.tags=$listTags ")
                    .WithParam("name", editRecipeRequest.name)
                    .WithParam("description", editRecipeRequest.description)
                    .WithParam("country", editRecipeRequest.country)
                    .WithParam("image", editRecipeRequest.image)
                    .WithParam("difficulty", editRecipeRequest.difficulty)
                    .WithParam("listIng", listIng)
                    .WithParam("listSteps", listSteps)
                    .WithParam("listTags",listTags)
                    .ExecuteWithoutResultsAsync();

                return Ok();

                // No se devuelve nada explícitamente, ya que se usa "async Task" como tipo de retorno
            }
            catch (Exception ex)
            {
                return BadRequest();
            }

        }



    }
}
