using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Neo4j.Driver;
using Neo4jClient;
using Neo4jClient.Cypher;
using Newtonsoft.Json.Linq;
using System;
using System.Diagnostics.Metrics;
using System.Globalization;
using System.Linq;
using System.Net.WebSockets;
using System.Text;
using TasteItApi.authentication;
using TasteItApi.Models;
using TasteItApi.Requests;
using static System.Net.Mime.MediaTypeNames;

namespace TasteItApi.Controllers
{
    [Authorize]
    [ApiController]
    [Route("controller")]
    public class RecipeController : Controller
    {
        private readonly IGraphClient _client;

        //DOC: https://github.com/DotNet4Neo4j/Neo4jClient/wiki

        // Diccionario de palabras más usadas en las recetas
        string[] commonWords = new string[] {
                    "sal", "azucar", "aceite", "cebolla",
                    "ajo", "tomate", "pollo", "carne", "pescado",
                    "arroz", "pasta", "huevo", "huevos", "leche", "harina",
                    "pan", "queso", "mayonesa", "mostaza", "vinagre",
                    "limon", "naranja", "manzana", "platano", "fresa",
                    "chocolate", "vainilla", "canela", "nuez", "mantequilla",
                    "crema", "almendra", "cacahuete", "mermelada", "miel",
                    "jengibre", "curry", "pimienta", "salvia", "romero",
                    "oregano", "laurel", "tomillo", "perejil", "cilantro",
                    "menta", "albahaca", "salsa", "sopa", "ensalada",
                    "guiso", "horneado", "frito", "asado", "cocido", "microondas" };


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

            //if (results.Count == 0)
            //{
            //    return NotFound();
            //}

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

            //var results = result.ToList();

            return Ok(result);
        }

        [HttpGet("/recipe/random/{limit}")]
        public async Task<ActionResult<Recipe>> GetRandomRecipesWithLimit(int limit)
        {

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

            return Ok(results);
        }

        [HttpGet("/recipe/byname/{name}/{skipper}")]
        public async Task<ActionResult<Recipe>> GetRecipeByName(string name, int skipper)
        {
            //devuelve recetas filtrando por nombre seguido del usuario que la creo
            var result = await _client.Cypher
                            .Match("(recipe:Recipe)-[:Created]-(user:User)")
                            //.Where((Recipe recipe) => recipe.name.Contains(name))
                            .Where("toLower(recipe.name) CONTAINS toLower($name)")
                            .WithParam("name", name)
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

            var recipe = result.ToList();

            return Ok(recipe);
        }

        [HttpGet("/recipe/bycountry/{country}/{skipper}")]
        public async Task<ActionResult<Recipe>> GetRecipeByCountry(string country, int skipper)
        {
            //filtramos recetas por ciudad seguido del usuario que la creo
            var result = await _client.Cypher
                            .Match("(recipe:Recipe)-[:Created]-(user:User)")
                            //.Where((Recipe recipe) => recipe.country.Contains(country))
                            .Where("toLower(recipe.country) CONTAINS toLower($country)")
                            .WithParam("country", country)
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

            var recipe = result.ToList();

            return Ok(recipe);
        }

        [HttpGet("/recipe/byuser/{token}/{skipper}")]
        public async Task<ActionResult<User>> GetRecipesByUser(string token, int skipper)
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
                .OrderBy("recipe.dateCreated desc")
                .Skip(skipper)
                .Limit(10)
                .ResultsAsync;

            var creators = result.ToList();

            return Ok(creators);
        }

        //ARREGLAR el skipper
        [HttpGet("/recipe/byingredients/{ingredients}/{skipper}")]
        public async Task<ActionResult<List<Recipe>>> GetRecipeByIngredients(string ingredients, int skipper)
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
                            .OrderBy("recipe.dateCreated desc")
                            .ResultsAsync;

            var recipes = result.ToList();

            //Dictionary<Recipe, User> listRecipesFiltered = new Dictionary<Recipe, User>();
            List<RecipeId_Recipe_User> listRecipesFiltered = new List<RecipeId_Recipe_User>();

            for (int i = 0; i < recipes.Count; i++)
            {
                //miramos si la lista de los ingredientes contiene alguno de los elementos de los introducidos por el usuario
                bool hasMatch = recipes[i].Recipe.ingredients.Any(x => listIng.Any(y => y.ToLower() == x.ToLower()));

                if (hasMatch)
                {
                    listRecipesFiltered.Add(new RecipeId_Recipe_User
                    {
                        RecipeId = recipes[i].RecipeId,
                        Recipe = recipes[i].Recipe.As<Recipe>(),
                        User = recipes[i].User.As<User>()
                    });
                }

            }

            return Ok(listRecipesFiltered.ToList());
        }

        //ARREGLAR el skipper
        [HttpGet("/recipe/bytags/{tags}/{skipper}")]
        public async Task<ActionResult<List<Recipe>>> GetRecipeByTags(string tags, int skipper)
        {

            List<string> listTags = tags.Replace(" ", "").Split(",").ToList();

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

            var recipes = result.ToList();

            //Dictionary<Recipe, User> listRecipesFiltered = new Dictionary<Recipe, User>();
            List<RecipeId_Recipe_User> listRecipesFiltered = new List<RecipeId_Recipe_User>();

            for (int i = 0; i < recipes.Count; i++)
            {
                //miramos si la lista de los ingredientes contiene alguno de los elementos de los introducidos por el usuario
                bool hasMatch = recipes[i].Recipe.ingredients.Any(x => listTags.Any(y => y.ToLower() == x.ToLower()));

                if (hasMatch)
                {
                    listRecipesFiltered.Add(new RecipeId_Recipe_User
                    {
                        RecipeId = recipes[i].RecipeId,
                        Recipe = recipes[i].Recipe.As<Recipe>(),
                        User = recipes[i].User.As<User>()
                    });
                }

            }

            return Ok(listRecipesFiltered.ToList());
        }

        private static string Normalize(string word)
        {
            return word.ToLowerInvariant().Normalize(NormalizationForm.FormD)
                       .Where(ch => CharUnicodeInfo.GetUnicodeCategory(ch) !=
                                     UnicodeCategory.NonSpacingMark)
                       .Aggregate(new StringBuilder(), (sb, ch) => sb.Append(ch))
                       .ToString().Normalize(NormalizationForm.FormC);
        }

        [AllowAnonymous]
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

                
                List<string> listIng = recipeRequest.ingredients.ToList();
                List<string> listSteps = recipeRequest.steps.ToList();
                List<string> listTags = new List<string>();

                // Obtener las palabras clave del nombre, descripción, steps e ingredientes
                string[] keywords = recipeRequest.name.Split(' ', StringSplitOptions.RemoveEmptyEntries)
                    .Concat(recipeRequest.description.Split(' ', StringSplitOptions.RemoveEmptyEntries))
                    .Concat(recipeRequest.ingredients.SelectMany(i => i.Split(' ', StringSplitOptions.RemoveEmptyEntries)))
                    .Concat(recipeRequest.steps.SelectMany(s => s.Split(' ', StringSplitOptions.RemoveEmptyEntries)))
                    .Select(w => Normalize(w.ToLower()))
                    //.Where(w => !commonWords.Contains(w))
                    .Distinct()
                    .ToArray();

                // Generar los tags a partir de las coincidencias con el diccionario de palabras comunes
                foreach (string word in keywords)
                {
                    if (commonWords.Contains(Normalize(word)))
                    {
                        listTags.Add(word);
                    }
                }

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
        public async Task<IActionResult> PostCommentRecipe([FromBody] CommentRecipeRequest commentRequest)
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

            bool isLiked;

            if(recipe.Count == 0)
            {
                isLiked = false;
            }else
            {
                isLiked = true;
            }

            return Ok(isLiked);
        }

        //DA O QUITA LIKE A UNA RECETA
        [HttpPost("/recipe/like")]
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

            return Ok(users);
        }

        //DEVUELVE LOS COMENTARIOS DE UNA RECETA
        [HttpGet("/recipe/comments/{rid}/{skipper}")]
        public async Task<IActionResult> GetCommentsOnRecipe(int rid, int skipper)
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
                    c = c.As<Comment>()

                })
                .OrderBy("c.dateCreated desc")
                .Skip(skipper)
                .Limit(10)
                .ResultsAsync;

            var comments = result.ToList();

            return Ok(comments);
        }


        //UPDATES

        //EDITAR RECETA
        [HttpPost("/recipe/edit")]
        public async Task<IActionResult> PostEditRecipe([FromBody] EditRecipeRequest request)
        {
            try
            {

                List<string> listIng = request.ingredients.ToList();
                List<string> listSteps = request.steps.ToList();
                List<string> listTags = new List<string>();

                // Obtener las palabras clave del nombre, descripción, steps e ingredientes
                string[] keywords = request.name.Split(' ', StringSplitOptions.RemoveEmptyEntries)
                    .Concat(request.description.Split(' ', StringSplitOptions.RemoveEmptyEntries))
                    .Concat(request.ingredients.SelectMany(i => i.Split(' ', StringSplitOptions.RemoveEmptyEntries)))
                    .Concat(request.steps.SelectMany(s => s.Split(' ', StringSplitOptions.RemoveEmptyEntries)))
                    .Select(w => Normalize(w.ToLower()))
                    //.Where(w => !commonWords.Contains(w))
                    .Distinct()
                    .ToArray();

                // Generar los tags a partir de las coincidencias con el diccionario de palabras comunes
                foreach (string word in keywords)
                {
                    if (commonWords.Contains(Normalize(word)))
                    {
                        listTags.Add(word);
                    }
                }

                //NOTA: hay que autogenerar los tags

                await _client.Cypher
                    .Match("(r:Recipe)")
                    .Where("Id(r)=" + request.rid)
                    .Set("r.name =$name,r.description=$description,r.country =$country,r.image=$image,r.difficulty=$difficulty,r.steps=$listSteps,r.ingredients=$listIng,r.tags=$listTags ")
                    .WithParam("name", request.name)
                    .WithParam("description", request.description)
                    .WithParam("country", request.country)
                    .WithParam("image", request.image)
                    .WithParam("difficulty", request.difficulty)
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


        [HttpGet("/recipe/check_owner/{rid}/{token}")]
        public async Task<IActionResult> GetheckOwnerRecipe(int rid, string token)
        {

            //token = "xmg10sMQgMS4392zORWGW7TQ1Qg2";

            var result = await _client.Cypher
                .Match("(recipe:Recipe)-[c:Created]-(u:User)")
                .Where("ID(recipe) = $rid")
                .WithParam("rid", rid)
                .Return((recipe, u) => new
                {
                    RecipeId = recipe.Id(),
                    Recipe = recipe.As<Recipe>(),
                    User = u.As<User>(),

                })
                .ResultsAsync;

            var results = result.ToList();

            bool confirmation = false;

            if (results[0].User.token.Equals(token))
            {
                confirmation = true;
            }

            return Ok(confirmation);
        }

    }

    


}
