using TasteItApi.Models;
using Microsoft.AspNetCore.Mvc;
using Neo4jClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TasteItApi.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class UserController : ControllerBase
    {

        private readonly IGraphClient _client;

        public UserController(IGraphClient client)
        {
            _client = client;
        }
        /*
        [HttpPost]
        public async Task<IActionResult> CreateUser([FromBody] User user)
        {
            await _client.Cypher.Create("(u:User $user)")
                                .WithParam("user", user)
                                .ExecuteWithoutResultsAsync();

            return Ok();
        }
        */
        [HttpGet("/byname/{username}")]
        public async Task<ActionResult<User>> GetUserByName(string username)
        {
            var result = await _client.Cypher
                        .Match("(u:User)")
                        .Where((User u) => u.username == username)
                        .Return(u => u.As<User>())
                        .ResultsAsync;


            var user = result.FirstOrDefault();

            if (user == null)
            {
                return NotFound();
            }

            return Ok(user);
        }

        //DEVUELVE EL USER SEGUN EL TOKEN
        [HttpGet("/bytoken/{token}")]
        public async Task<ActionResult<User>> GetUserByToken(string token)
        {
            var result = await _client.Cypher
                        .Match("(u:User)")
                        .Where((User u) => u.token == token)
                        .Return(u => u.As<User>())
                        .ResultsAsync;


            var user = result.FirstOrDefault();

            if (user == null)
            {
                return NotFound();
            }

            return Ok(user);
        }

        //NUMERO DE LIKES DEL USUARIO
        [HttpGet("/likes")]
        public async Task<IActionResult> GetRecipesLiked(string token)
        {

            //token = "xmg10sMQgMS4392zORWGW7TQ1Qg2";

            var result = await _client.Cypher
                .Match("(u:User)-[c:Liked]->(r:Recipe)")
                .Where("u.token = $token")
                .WithParam("token", token)
                .Return(r => r.As<Recipe>())
                .ResultsAsync;

            var recipes = result.ToList();

            if (recipes.Count == 0)
            {
                return NotFound();
            }

            return Ok(recipes);
        }

        //RECETAS DE TUS SEGUIDORES
        [HttpGet("/followed/recipes")]
        public async Task<IActionResult> GetRecipesFollowed(string token)
        {

            //token = "xmg10sMQgMS4392zORWGW7TQ1Qg2";

            var result = await _client.Cypher
                .Match("(u:User)-[c:Liked]->(r:Recipe)")
                .Where("u.token = $token")
                .WithParam("token", token)
                .Return(r => r.As<Recipe>())
                .ResultsAsync;

            var recipes = result.ToList();

            if (recipes.Count == 0)
            {
                return NotFound();
            }

            return Ok(recipes);
        }

        [HttpPost("/edit")]
        public async Task<IActionResult> PostChangesOnUser(string token, string username, string imgProfile, string biography)
        {

            //token = "xmg10sMQgMS4392zORWGW7TQ1Qg2";

            var result = await _client.Cypher
               .Match("(u:User)")
               .Where("u.token = $token")
               .Set("u.username = $username")
               .Set("u.imgProfile = $imgProfile")
               .Set("u.biography = $biography")
               .WithParam("token", token)
               .WithParam("username", username)
               .WithParam("imgProfile", imgProfile)
               .WithParam("biography", biography)
               .Return(u => u.As<User>())
               .ResultsAsync;

            return Ok(result);
        }
        //usuario A empieza a seguir al usuario B, o lo quita
        [HttpPost("/follow/{receiverToken}")]
        public async Task<IActionResult> PostFollowUser(string senderToken, string receiverToken)
        {

            //tokenManolo = "xmg10sMQgMS4392zORWGW7TQ1Qg2";
            //tokenPepito = "ZdoWamcZHHT26CG9IM7tKnze3ul2";

            string today = DateTime.Today.ToShortDateString();

            var result = await _client.Cypher
               .Match("(sender:User)-[follow:Following]->(receiver:User)")
               .Where("sender.token = $senderToken")
               .AndWhere("receiver.token = $receiverToken")
               .WithParam("senderToken", senderToken)
               .WithParam("receiverToken", receiverToken)
               .Return((sender, follow, receiver) => new
               {
                   sender = sender.As<Recipe>(),
                   follow = follow.As<Follow>(),
                   receiver = receiver.As<User>()

               })
               .ResultsAsync;

            var users = result.ToList();

            if (users.Count != 0) //si no es 0 por lo tanto existe y lo borra
            {
                result = await _client.Cypher
               .Match("(sender:User)-[follow:Following]->(receiver:User)")
               .Where("sender.token = $senderToken")
               .AndWhere("receiver.token = $receiverToken")
               .Delete("follow")
               .WithParam("senderToken", senderToken)
               .WithParam("receiverToken", receiverToken)
               .Return((sender, follow, receiver) => new
               {
                   sender = sender.As<Recipe>(),
                   follow = follow.As<Follow>(),
                   receiver = receiver.As<User>()
               })
               .ResultsAsync;

                users = result.ToList();
            }
            else // crea la relacion
            {
                result = await _client.Cypher
               .Match("(sender:User),(receiver:User)")
               .Where("sender.token = $senderToken")
               .AndWhere("receiver.token = $receiverToken")
               .Create("(sender)-[follow:Following{dateCreated:$dateCreated}]->(receiver)")
               .WithParam("senderToken", senderToken)
               .WithParam("receiverToken", receiverToken)
               .WithParam("dateCreated", today)
               .Return((sender, follow, receiver) => new
               {
                   sender = sender.As<Recipe>(),
                   follow = follow.As<Follow>(),
                   receiver = receiver.As<User>()

               })
               .ResultsAsync;

                users = result.ToList();
            }



            return Ok(users);
        }






    }
}
