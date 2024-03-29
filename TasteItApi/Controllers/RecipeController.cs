﻿using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Neo4j.Driver;
using Neo4jClient;
using Neo4jClient.Cypher;
using Newtonsoft.Json.Linq;
using System;
using System.Diagnostics.Metrics;
using System.Globalization;
using System.Linq;
using System.Net.Http.Headers;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
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
        string[] commonWords = new string[] 
            {
                "sal", "azucar", "aceite", "cebolla",
                "ajo", "tomate", "pollo", "carne", "pescado",
                "arroz", "pasta", "huevo", "leche", "harina",
                "pan", "queso", "mayonesa", "mostaza", "vinagre",
                "limon", "naranja", "manzana", "platano", "fresa",
                "chocolate", "vainilla", "canela", "nuez", "mantequilla",
                "crema", "almendra", "cacahuete", "mermelada", "miel",
                "jengibre", "curry", "pimienta", "salvia", "romero",
                "oregano", "laurel", "tomillo", "perejil", "cilantro",
                "menta", "albahaca", "salsa", "sopa", "ensalada",
                "guiso", "horneado", "frito", "asado", "cocido", "microondas", 
                "pastel", "tarta", "fruta", "yogurt", "aguacate", "sopa"
            };


        public RecipeController(IGraphClient client)
        {
            _client = client;
        }

        //devuelve las recetas seguido del usuario que la creo
        [HttpGet("/recipe/all")]
        public async Task<ActionResult<RecipeId_Recipe_User>> GetAllRecipes()
        {
            try
            {
                var query = await _client.Cypher
                    .Match("(recipe:Recipe)-[:Created]-(user:User)")
                    .Where("recipe.active = true")
                    .Return((recipe, user) => new
                    {
                        RecipeId = recipe.Id(),
                        Recipe = recipe.As<Recipe>(),
                        User = user.As<User>()
                    })
                    .OrderBy("recipe.dateCreated desc")
                .ResultsAsync;

                var results = query.ToList();

                return Ok(results);
            }
            catch (Exception ex)
            {
                return BadRequest(ex);
            }
            
        }

        //devuelve las recetas seguido del usuario que la creo con paginacion
        [HttpGet("/recipe/all/{skipper:int}")]
        public async Task<ActionResult<Recipe>> GetAllRecipesWithSkipper(int skipper)
        {
            try
            {
                var query = await _client.Cypher
                    .Match("(recipe:Recipe)-[:Created]-(user:User)")
                    .Where("recipe.active = true")
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

                var results = query.ToList();

                return Ok(results);
            }
            catch (Exception ex)
            {
                return BadRequest(ex);
            }
        }

        //devuelve la receta filtrando por id seguido del usuario que la creo
        [HttpGet("/recipe/{id:int}")]
        public async Task<ActionResult<Recipe>> GetRecipeById(int id)
        {
            try
            {
                var query = await _client.Cypher
                   .Match("(user:User)-[:Created]-(recipe:Recipe)")
                   .Where("ID(recipe) = " + id)
                   .AndWhere("recipe.active = true")
                   .Return((recipe, user) => new
                   {
                       RecipeId = recipe.Id(),
                       Recipe = recipe.As<Recipe>(),
                       User = user.As<User>()
                   })
               .ResultsAsync;

                var results = query.ToList();

                return Ok(results);

            } catch(Exception ex)
            {
                return BadRequest(ex);
            }
        }
        //devuelve un numero especifico de recetas aleatorias seguido del usuario que la creo
        [HttpGet("/recipe/random/{limit}")]
        public async Task<ActionResult<Recipe>> GetRandomRecipesWithLimit(int limit)
        {
            try
            {
                var query = await _client.Cypher
                    .Match("(user:User)-[:Created]-(recipe:Recipe)")
                    .Where("recipe.active = true")
                    .With("recipe, rand() as rand")
                    .OrderBy("rand limit $limit")
                    .Match("(user:User)-[:Created]-(recipe:Recipe)")
                    .WithParam("limit", limit)
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
            catch (Exception ex)
            {
                return BadRequest(ex);
            }
        }

        //devuelve recetas filtrando por nombre seguido del usuario que la creo
        [HttpGet("/recipe/byname/{name}/{skipper}")]
        public async Task<ActionResult<Recipe>> GetRecipeByName(string name, int skipper)
        {
            try
            {
                var query = await _client.Cypher
                    .Match("(recipe:Recipe)-[:Created]-(user:User)")
                    .Where("toLower(recipe.name) CONTAINS toLower($name)")
                    .AndWhere("recipe.active = true")
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

                var results = query.ToList();

                return Ok(results);
            }
            catch(Exception ex)
            {
                return BadRequest(ex);
            }
        }

        //filtramos recetas filtrando por ciudad seguido del usuario que la creo
        [HttpGet("/recipe/bycountry/{country}/{skipper}")]
        public async Task<ActionResult<Recipe>> GetRecipeByCountry(string country, int skipper)
        {
            try
            {
                var query = await _client.Cypher
                    .Match("(recipe:Recipe)-[:Created]-(user:User)")
                    .Where("toLower(recipe.country) CONTAINS toLower($country)")
                    .AndWhere("recipe.active = true")
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

                var results = query.ToList();

                return Ok(results);

            } catch (Exception ex)
            {
                return BadRequest(ex);
            }
        }

        //devuelve las recetas de un usuario
        [HttpGet("/recipe/byuser/{token}/{skipper}")]
        public async Task<ActionResult<User>> GetRecipesByUser(string token, int skipper)
        {
            try
            {
                var query = await _client.Cypher
                    .Match("(user:User)-[:Created]-(recipe:Recipe)")
                    .Where((User user) => user.token == token)
                    .AndWhere("recipe.active = true")
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

                var results = query.ToList();

                return Ok(results);
            }
            catch(Exception ex)
            {
                return BadRequest(ex);
            }
        }

        //devuelve recetas por ingredientes
        [HttpGet("/recipe/byingredients/{ingredients}/{skipper}")]
        public async Task<ActionResult<List<Recipe>>> GetRecipeByIngredients(string ingredients, int skipper)
        {
            try
            {
                List<string> listIng = ingredients.Replace(" ", "").Split(",").ToList();

                var query = await _client.Cypher
                    .Match("(recipe:Recipe)-[:Created]-(user:User)")
                    .Where("recipe.active = true")
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

                var recipes = query.ToList();

                List<RecipeId_Recipe_User> listRecipesFiltered = new List<RecipeId_Recipe_User>();

                for (int i = 0; i < recipes.Count; i++)
                {
                    //miramos si la lista de los ingredientes contiene alguno de los elementos de los introducidos por el usuario
                    bool hasMatch = recipes[i].Recipe.ingredients.Any(x => listIng.Any(y => x.ToLower().Contains(y.ToLower())));

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

            } catch(Exception ex)
            {
                return BadRequest(ex);
            }
        }

        //devuelve recetas por tags
        [HttpGet("/recipe/bytags/{tags}/{skipper}")]
        public async Task<ActionResult<List<Recipe>>> GetRecipeByTags(string tags, int skipper)
        {
            try
            {
                List<string> listTags = tags.Replace(" ", "").Split(",").ToList();

                var result = await _client.Cypher
                    .Match("(recipe:Recipe)-[:Created]-(user:User)")
                    .Where("recipe.active = true")
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

                var recipes = result.ToList();

                List<RecipeId_Recipe_User> listRecipesFiltered = new List<RecipeId_Recipe_User>();

                for (int i = 0; i < recipes.Count; i++)
                {
                    //miramos si la lista de los ingredientes contiene alguno de los elementos de los introducidos por el usuario
                    bool hasMatch = recipes[i].Recipe.tags.Any(x => listTags.Any(y => y.ToLower() == x.ToLower()));

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

            } catch(Exception ex)
            {
                return BadRequest(ex);
            }
        }

        private static string Normalize(string word)
        {
            return word.ToLowerInvariant().Normalize(NormalizationForm.FormD)
                       .Where(ch => CharUnicodeInfo.GetUnicodeCategory(ch) !=
                                     UnicodeCategory.NonSpacingMark)
                       .Aggregate(new StringBuilder(), (sb, ch) => sb.Append(ch))
                       .ToString().Normalize(NormalizationForm.FormC);
        }

        //CREAR RECETA
        [HttpPost("/recipe/create")]
        public async Task<IActionResult> PostCreateRecipe([FromBody] RecipeRequest recipeRequest)
        {
            try
            {
                DateTime today = DateTime.Now;

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
                    .Create("(recipe:Recipe {name:$name,description:$description,country:$country,dateCreated:$dateCreated,image:$image,difficulty:$difficulty,steps:$steps,ingredients:$ingredients,tags:$tags,rating:0.0,active:true})-[c:Created]->(user)")
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
        public async Task<IActionResult> PostCommentRecipe([FromBody] CommentRecipeRequest request)
        {
            DateTime today = DateTime.Now;
            
            var result = await _client.Cypher
                .Match("(user:User),(recipe:Recipe)")
                .Where("ID(recipe) = $rid")
                .AndWhere("user.token = $token")
                .Create("(user)-[cmt:Commented{ comment:$comment, rating:$rating, dateCreated:$dateCreated}]->(recipe)")
                .WithParam("token", request.token)
                .WithParam("rid", request.rid)
                .WithParam("comment", request.comment)
                .WithParam("rating", request.rating)
                .WithParam("dateCreated", today)
                .Return((recipe, user, cmt) => new
                {
                    RecipeId = recipe.Id(),
                    Recipe = recipe.As<Recipe>(),
                    User = user.As<User>(),
                    cmt = cmt.As<Comment>()
                })
                .ResultsAsync;
            
            await updateRatingRecipeAsync(request.rid);

            return Ok();
        }

        //actualizar el rating de una receta tras un comentario
        private async Task updateRatingRecipeAsync(int recipeId)
        {
            var total = 0.0;

            var query = await _client.Cypher
                .Match("(recipe:Recipe)-[c:Commented]-(u:User)")
                .Where("ID(recipe) = $rid")
                .WithParam("rid", recipeId)
                .Return((c,u) => new
                {
                    user = u.As<User>(),
                    comment = c.As<Comment>()
                })
                .OrderBy("u.token, c.dateCreated asc")
            .ResultsAsync;

            var results = query.ToList();

            var comments = new Dictionary<string,double>();

            foreach(var result in results)
            {
                if(!comments.ContainsKey(result.user.token))
                comments.Add(result.user.token, result.comment.rating);
            }

            foreach (var comment in comments)
            {
                total += comment.Value;
            }

            total = total / comments.Count;
  
            await _client.Cypher
                .Match("(r:Recipe)")
                .Where("ID(r)= $rid")
                .Set("r.rating = $total")
                .WithParam("rid", recipeId)
                .WithParam("total", total)
            .ExecuteWithoutResultsAsync();
            
        }

        //REPORTAR UNA RECETA
        [HttpPost("/recipe/report_recipe")]
        public async Task<IActionResult> PostReportRecipe([FromBody] ReportRecipeRequest reportRecipeRequest)
        {
            DateTime today = DateTime.Now;

            var query = await _client.Cypher
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

            var results = query.ToList();

            return Ok(results);
        }

        //CONFIRMAR SI UNA RECETA TIENE ME GUSTA
        [HttpGet("/recipe/isliked/{rid}_{token}")]
        public async Task<IActionResult> GetRecipeIsLiked(int rid, string token)
        {
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

            if (recipe.Count == 0)
            {
                isLiked = false;
            }
            else
            {
                isLiked = true;
            }

            return Ok(isLiked);
        }

        //DA O QUITA LIKE A UNA RECETA
        [HttpPost("/recipe/like")]
        public async Task<IActionResult> PostLikeOnRecipe(LikeRecipeRequest likeRecipeRequest)
        {
            DateTime today = DateTime.Now;

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
            try
            {
                var query = await _client.Cypher
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

                var results = query.ToList();

                return Ok(results);

            } catch(Exception ex)
            {
                return BadRequest(ex);
            }
        }

        //DEVUELVE LOS COMENTARIOS DE UNA RECETA
        [HttpGet("/recipe/comments/{rid}/{skipper}")]
        public async Task<IActionResult> GetCommentsOnRecipe(int rid, int skipper)
        {
            try
            {
                var query = await _client.Cypher
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

                var comments = query.ToList();

                return Ok(comments);
            }
            catch(Exception ex)
            {
                return BadRequest(ex);
            }
        }

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

                await _client.Cypher
                    .Match("(r:Recipe)")
                    .Where("Id(r)=" + request.rid)
                    .Set("r.name =$name,r.description=$description,r.country=$country,r.image=$image,r.difficulty=$difficulty,r.steps=$listSteps,r.ingredients=$listIng,r.tags=$listTags ")
                    .WithParam("name", request.name)
                    .WithParam("description", request.description)
                    .WithParam("country", request.country)
                    .WithParam("image", request.image)
                    .WithParam("difficulty", request.difficulty)
                    .WithParam("listIng", listIng)
                    .WithParam("listSteps", listSteps)
                    .WithParam("listTags", listTags)
                    .ExecuteWithoutResultsAsync();

                return Ok();

                // No se devuelve nada explícitamente, ya que se usa "async Task" como tipo de retorno
            }
            catch (Exception ex)
            {
                return BadRequest(ex);
            }

        }

        //confirma el propietario de una receta
        [HttpGet("/recipe/check_owner/{rid}/{token}")]
        public async Task<IActionResult> GetheckOwnerRecipe(int rid, string token)
        {

            var query = await _client.Cypher
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

            var results = query.ToList();

            bool confirmation = false;

            if (results[0].User.token.Equals(token))
            {
                confirmation = true;
            }

            return Ok(confirmation);
        }

        //borrar una receta
        [HttpPost("/recipe/delete")]
        public async Task<IActionResult> PostDeleteRecipe([FromBody] DeleteRecipeRequest request)
        {
            await _client.Cypher
                .Match("(r:Recipe)")
                .Where("ID(r) = $recipeId")
                .DetachDelete("r")
                .WithParam("recipeId", request.RecipeId)
                .ExecuteWithoutResultsAsync();

            return Ok();
        }

        //devuelve el numero de likes de una receta
        [HttpGet("/recipe/likes_on_recipes/{rid}")]
        public async Task<IActionResult> GetCountLikesOnRecipe(int rid)
        {
            var result = await _client.Cypher
                .Match("(u:User)-[count:Liked]->(r:Recipe)")
                .Where("ID(r) = $rid")
                .WithParam("rid", rid)
                .Return(r => r.Count())
            .ResultsAsync;

            return Ok(result);
        }

        //buscador
        [HttpGet("/recipe/search")]
        public async Task<ActionResult<RecipeId_Recipe_User>> GetRecipesFiltered(string? name, string? country, int? difficulty, float? rating, string? ingredients, string? tags)
        {
            List<string> listIng = new List<string>();
            List<string> listTags = new List<string>();
            double lowestRating = 0.0;
            double highestRating = 0.0;

            if (ingredients != null)
            {
                listIng = ingredients.Replace(" ", "").Split(",").ToList();
            }

            if (tags != null)
            {
                listTags = tags.Replace(" ", "").Split(",").ToList();
            }

            if(rating != null)
            {
                lowestRating = Math.Floor((float)rating);
                highestRating = lowestRating + 0.9;
            }

            var result = await _client.Cypher
                .Match("(recipe:Recipe)-[c:Created]-(user:User)")
                .Where("($name IS NULL OR toLower(recipe.name) CONTAINS toLower($name))")
                .AndWhere("($country IS NULL OR toLower(recipe.country) CONTAINS toLower($country))")
                .AndWhere("($difficulty IS NULL OR recipe.difficulty = $difficulty)")
                .AndWhere("($rating IS NULL OR (recipe.rating >= $lowestRating AND recipe.rating <= $highestRating))")
                .AndWhere("ALL(tag IN $tags WHERE tag IN recipe.tags)")
                .AndWhere("recipe.active = true")
                .WithParam("name", name)
                .WithParam("country", country)
                .WithParam("difficulty", difficulty)
                .WithParam("rating", rating)
                .WithParam("lowestRating", lowestRating)
                .WithParam("highestRating", highestRating)
                .WithParam("tags", listTags)
                .Return((recipe, user) => new
                {
                    RecipeId = recipe.Id(),
                    Recipe = recipe.As<Recipe>(),
                    User = user.As<User>()
                })
                .OrderBy("recipe.dateCreated desc")
            .ResultsAsync;
            
            var recipes = result.ToList();
            
            List<RecipeId_Recipe_User> listRecipesFiltered = new List<RecipeId_Recipe_User>();

            if (listIng.Count > 0) // Filtrar solo por ingredientes
            {
                for (int i = 0; i < recipes.Count; i++)
                {
                    String pepito = String.Join(" ", recipes[i].Recipe.ingredients);

                    bool hasMatch = listIng.All(x => pepito.Contains(x));

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
            }
           
            if (listIng.Count > 0)
            {
                return Ok(listRecipesFiltered);
            }
            else
            {
                return Ok(recipes);
            }
            
        }

    }

}
