using TasteItApi.Models;
using Microsoft.AspNetCore.Mvc;
using Neo4jClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TasteItApi.Requests;
using Microsoft.AspNetCore.Authorization;

namespace TasteItApi.Controllers
{
    //[Authorize]
    [ApiController]
    [Route("[controller]")]
    public class UserController : ControllerBase
    {

        private readonly IGraphClient _client;

        public UserController(IGraphClient client)
        {
            _client = client;
        }
        
        [HttpPost("/user/register")]
        public async Task<IActionResult> PostRegisterUser([FromBody] User user)
        {
            await _client.Cypher.Create("(u:User $user)")
                                .WithParam("user", user)
                                .ExecuteWithoutResultsAsync();

            return Ok();
        }
        
        [HttpGet("/user/byname/{username}/{skipper:int}")]
        public async Task<ActionResult<User>> GetUserByName(string username, int skipper)
        {
            var result = await _client.Cypher
                        .Match("(u:User)")
                        .Where((User u) => u.username.Contains(username))
                        .Return(u => u.As<User>())
                        .OrderBy("u.username desc")
                        .Skip(skipper)
                        .Limit(10)
                        .ResultsAsync;


            var users = result.ToList();

            //if (user == null)
            //{
            //    return NotFound();
            //}

            return Ok(users);
        }

        //DEVUELVE EL USER SEGUN EL TOKEN
        [HttpGet("/user/bytoken/{token}")]
        public async Task<ActionResult<User>> GetUserByToken(string token)
        {
            var result = await _client.Cypher
                        .Match("(u:User)")
                        .Where((User u) => u.token == token)
                        .Return(u => u.As<User>())
                        .ResultsAsync;


            var user = result.FirstOrDefault();

            return Ok(user);
        }

        //DEVUELVE LAS RECETAS  QUE A UN USUARIO LE GUSTAN
        [HttpGet("/user/liked_recipes/{token}/{skipper}")]
        public async Task<IActionResult> GetRecipesLiked(string token, int skipper)
        {

            //token = "xmg10sMQgMS4392zORWGW7TQ1Qg2";

            var result = await _client.Cypher
                .Match("(u:User)-[l:Liked]->(r:Recipe)-[c:Created]->(u2:User) ")
                .Where("u.token = $token")
                .WithParam("token", token)
                .Return((r, u2) => new
                {
                    RecipeId = r.Id(),
                    Recipe = r.As<Recipe>(),
                    User = u2.As<User>()
                })
                .OrderBy("r.dateCreated desc")
                .Skip(skipper)
                .Limit(10)
                .ResultsAsync;

            var recipes = result.ToList();

            return Ok(recipes);
        }

        //devuelve las RECETAS DE TUS SEGUIDORES
        [HttpGet("/user/followers_recipes/{token}/{skipper}")]
        public async Task<IActionResult> GetRecipesFollowed(string token, int skipper)
        {

            //token = "xmg10sMQgMS4392zORWGW7TQ1Qg2";

            var result = await _client.Cypher
                .Match("(u1:User)-[f:Following]->(u2:User)")
                .Where("u1.token = $token")
                .Match("(r:Recipe)-[c:Created]->(u2)")
                .WithParam("token", token)
                .Return((r, u2) => new
                {
                    RecipeId = r.Id(),
                    Recipe = r.As<Recipe>(),
                    User = u2.As<User>()
                })
                .OrderBy("r.dateCreated desc")
                .Skip(skipper)
                .Limit(10)
                .ResultsAsync;

            var recipes = result.ToList();

            return Ok(recipes);
        }

        [HttpPost("/user/edit")]
        public async Task<IActionResult> PostChangesOnUser([FromBody] EditUserRequest editUserRequest)
        {

            //token = "xmg10sMQgMS4392zORWGW7TQ1Qg2";

            var result = await _client.Cypher
               .Match("(u:User)")
               .Where("u.token = $token")
               .Set("u.username = $username")
               .Set("u.imgProfile = $imgProfile")
               .Set("u.biography = $biography")
               .WithParam("token", editUserRequest.token)
               .WithParam("username", editUserRequest.username)
               .WithParam("imgProfile", editUserRequest.imgProfile)
               .WithParam("biography", editUserRequest.biography)
               .Return(u => u.As<User>())
               .ResultsAsync;

            return Ok(result);
        }
        //usuario A empieza a seguir al usuario B, o lo quita
        [HttpPost("/user/follow")]
        public async Task<IActionResult> PostFollowUser([FromBody] FollowUserRequest followUserRequest)
        {

            //tokenManolo = "xmg10sMQgMS4392zORWGW7TQ1Qg2";
            //tokenPepito = "ZdoWamcZHHT26CG9IM7tKnze3ul2";

            string today = DateTime.Today.ToShortDateString();

            var result = await _client.Cypher
               .Match("(sender:User)-[follow:Following]->(receiver:User)")
               .Where("sender.token = $senderToken")
               .AndWhere("receiver.token = $receiverToken")
               .WithParam("senderToken", followUserRequest.senderToken)
               .WithParam("receiverToken", followUserRequest.receiverToken)
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
               .WithParam("senderToken", followUserRequest.senderToken)
               .WithParam("receiverToken", followUserRequest.receiverToken)
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
               .WithParam("senderToken", followUserRequest.senderToken)
               .WithParam("receiverToken", followUserRequest.receiverToken)
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

        [HttpPost("/user/comment_user")]
        public async Task<IActionResult> PostCommentUser([FromBody] CommentUserRequest request)
        {
            string today = DateTime.Today.ToShortDateString();
            //token = "xmg10sMQgMS4392zORWGW7TQ1Qg2";

            await _client.Cypher
                .Match("(u1:User),(u2:User)")
                .Where("u1.token = $senderId")
                .AndWhere("u2.token = $receiverId")
                .Create("(u1)-[c:Commented{comment: $comment, dateCreated: $todayDate}]->(u2)")
                .WithParam("senderId", request.SenderId)
                .WithParam("receiverId",request.ReceiverId)
                .WithParam("comment",request.Comment)
                .WithParam("todayDate",today)
                .ExecuteWithoutResultsAsync();

            return Ok();
        }

        [HttpGet("/user/recipes_created/{token}")]
        public async Task<IActionResult> GetCountRecipes(string token)
        {
            //token = "xmg10sMQgMS4392zORWGW7TQ1Qg2";

            var result = await _client.Cypher
                .Match("(u:User)-[count:Created]-(r:Recipe)")
                .Where("u.token = $token")
                .WithParam("token", token)
                .Return(count => count.Count())
                .ResultsAsync;

            return Ok(result);     
        }

        [HttpGet("/user/following/{token}")]
        public async Task<IActionResult> GetCountFollowing(string token)
        {
            //token = "xmg10sMQgMS4392zORWGW7TQ1Qg2";

            var result = await _client.Cypher
                .Match("(u:User)-[count:Following]->(u2:User)")
                .Where("u.token = $token")
                .WithParam("token", token)
                .Return(u2 => u2.Count())
                .ResultsAsync;

            return Ok(result);
        }

        [HttpGet("/user/followers/{token}")]
        public async Task<IActionResult> GetCountFollowers(string token)
        {
            //token = "xmg10sMQgMS4392zORWGW7TQ1Qg2";

            var result = await _client.Cypher
                .Match("(u:User)<-[count:Following]-(u2:User)")
                .Where("u.token = $token")
                .WithParam("token", token)
                .Return(u2 => u2.Count())
                .ResultsAsync;

            return Ok(result);
        }

        [HttpGet("/user/recipes_liked/{token}")]
        public async Task<IActionResult> GetCountLiked(string token)
        {
            //token = "xmg10sMQgMS4392zORWGW7TQ1Qg2";

            var result = await _client.Cypher
                .Match("(u:User)-[count:Liked]->(r:Recipe)")
                .Where("u.token = $token")
                .WithParam("token", token)
                .Return(r => r.Count())
                .ResultsAsync;

            return Ok(result);
        }

        [HttpGet("/user/comments/{token}")]
        public async Task<IActionResult> GetCommentsOnUser(string token)
        {
            //token = "xmg10sMQgMS4392zORWGW7TQ1Qg2";

            var result = await _client.Cypher
                .Match("(user:User)-[comment:Commented]->(u2:User)")
                .Where("u2.token = $token")
                .WithParam("token", token)
                .Return((user,comment) => new
                {
                    user = user.As<User>(),
                    comment = comment.As<CommentOnUser>()
                })
                .ResultsAsync;

            return Ok(result);
        }


    }
}
