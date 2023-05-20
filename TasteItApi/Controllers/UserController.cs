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
    [Authorize]
    [ApiController]
    [Route("[controller]")]
    public class UserController : ControllerBase
    {

        private readonly IGraphClient _client;

        public UserController(IGraphClient client)
        {
            _client = client;
        }

        //registrar usuarios en neo
        [AllowAnonymous]
        [HttpPost("/user/register")]
        public async Task<IActionResult> PostRegisterUser([FromBody] User user)
        {
            try
            {
                await _client.Cypher.Create("(u:User $user)")
                    .WithParam("user", user)
                .ExecuteWithoutResultsAsync();

                return Ok();
            }
            catch (Exception ex)
            {
                return BadRequest(ex);
            }
        }
        
        //devolver usuario por nombre
        [HttpGet("/user/byname/{username}/{skipper:int}")]
        public async Task<ActionResult<User>> GetUserByName(string username, int skipper)
        {
            try
            {
                var query = await _client.Cypher
                    .Match("(u:User)")
                    .Where("toLower(u.username) CONTAINS toLower($username)")
                    .WithParam("username", username)
                    .Return(u => u.As<User>())
                    .OrderBy("u.username desc")
                    .Skip(skipper)
                    .Limit(10)
                .ResultsAsync;

                var users = query.ToList();

                return Ok(users);
            }
            catch (Exception ex)
            {
                return BadRequest(ex);
            }
        }

        //DEVUELVE EL USER SEGUN EL TOKEN
        [HttpGet("/user/bytoken/{token}")]
        public async Task<ActionResult<User>> GetUserByToken(string token)
        {
            try
            {
                var result = await _client.Cypher
                    .Match("(u:User)")
                    .Where((User u) => u.token == token)
                    .Return(u => u.As<User>())
                .ResultsAsync;

                var user = result.FirstOrDefault();

                return Ok(user);
            }
            catch (Exception ex) 
            { 
                return BadRequest(ex); 
            }
        }

        //para web -- devuelve el usuario por token
        [HttpGet("/user/bytoken-web/{token}")]
        public async Task<ActionResult<User>> GetUserByTokenWEB(string token)
        {
            try
            {
                var query = await _client.Cypher
                    .Match("(u:User)")
                    .Where((UserWEB u) => u.token == token)
                    .Return(u => u.As<UserWEB>())
                .ResultsAsync;

                var user = query.FirstOrDefault();

                return Ok(user);

            } catch(Exception ex)
            {
                return BadRequest(ex);
            }
        }

        //DEVUELVE LAS RECETAS  QUE A UN USUARIO LE GUSTAN
        [HttpGet("/user/liked_recipes/{token}/{skipper}")]
        public async Task<IActionResult> GetRecipesLiked(string token, int skipper)
        {
            try
            {
                var query = await _client.Cypher
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

                var recipes = query.ToList();

                return Ok(recipes);
            }
            catch (Exception ex)
            {
                return BadRequest(ex);
            }
        }

        //devuelve las RECETAS DE TUS SEGUIDORES
        [HttpGet("/user/followers_recipes/{token}/{skipper}")]
        public async Task<IActionResult> GetRecipesFollowed(string token, int skipper)
        {
            try
            {
                var query = await _client.Cypher
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

                var recipes = query.ToList();

                return Ok(recipes);
            } 
            catch (Exception ex)
            {
                return BadRequest(ex);
            }
        }

        //edicion de usuario
        [HttpPost("/user/edit")]
        public async Task<IActionResult> PostChangesOnUser([FromBody] EditUserRequest editUserRequest)
        {
            try
            {
                var query = await _client.Cypher
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

                return Ok(query);
            } 
            catch(Exception ex)
            {
                return BadRequest(ex);
            }
        }
        
        //CONFIRMAR SI UN USUARIO SIGUE A OTRO USUARIO
        [HttpGet("/user/following/{sender_token}_{receiver_token}")]
        public async Task<IActionResult> GetIsFollowingUser(string sender_token, string receiver_token)
        {
            try
            {
                var result = await _client.Cypher
                    .Match("(sender:User)-[follow:Following]->(receiver:User)")
                    .Where("sender.token = $sender_token")
                    .AndWhere("receiver.token = $receiver_token")
                    .WithParam("sender_token", sender_token)
                    .WithParam("receiver_token", receiver_token)
                    .Return((follow) => new
                    {
                        follow = follow.As<Follow>()
                    })
                .ResultsAsync;

                bool isFollowing;

                if (result.ToList().Count == 0)
                {
                    isFollowing = false;
                }
                else
                {
                    isFollowing = true;
                }

                return Ok(isFollowing);
            }
            catch (Exception ex)
            {
                return BadRequest(ex);
            }
        }

        //Devolver a quien sigue un usuario
        [HttpGet("/user/following_user/{sender_token}/{skipper}")]
        public async Task<IActionResult> GetFollowingUser(string sender_token, int skipper)
        {
            try
            {
                var query = await _client.Cypher
                    .Match("(sender:User)-[follow:Following]->(receiver:User)")
                    .Where("sender.token = $sender_token")
                    .WithParam("sender_token", sender_token)
                    .Return(receiver => receiver.As<User>())
                    .OrderBy("follow.dateCreated desc")
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

        //Devolver a los seguidores de un usuario
        [HttpGet("/user/followers_user/{sender_token}/{skipper}")]
        public async Task<IActionResult> GetFollowersUser(string sender_token, int skipper)
        {
            try
            {
                var query = await _client.Cypher
                    .Match("(sender:User)<-[follow:Following]-(receiver:User)")
                    .Where("sender.token = $sender_token")
                    .WithParam("sender_token", sender_token)
                    .Return(receiver => receiver.As<User>())
                    .OrderBy("follow.dateCreated desc")
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

        //usuario A empieza a seguir al usuario B, o lo quita
        [HttpPost("/user/follow")]
        public async Task<IActionResult> PostFollowUser([FromBody] FollowUserRequest followUserRequest)
        {
            try
            {
                DateTime today = DateTime.Now;

                var query = await _client.Cypher
                   .Match("(sender:User)-[follow:Following]->(receiver:User)")
                   .Where("sender.token = $senderToken")
                   .AndWhere("receiver.token = $receiverToken")
                   .WithParam("senderToken", followUserRequest.senderToken)
                   .WithParam("receiverToken", followUserRequest.receiverToken)
                   .Return((sender, receiver) => new
                   {
                       sender = sender.As<Recipe>(),
                       receiver = receiver.As<User>()

                   })
                .ResultsAsync;

                var users = query.ToList();

                if (users.Count != 0) //si no es 0 por lo tanto existe y lo borra
                {
                    query = await _client.Cypher
                       .Match("(sender:User)-[follow:Following]->(receiver:User)")
                       .Where("sender.token = $senderToken")
                       .AndWhere("receiver.token = $receiverToken")
                       .Delete("follow")
                       .WithParam("senderToken", followUserRequest.senderToken)
                       .WithParam("receiverToken", followUserRequest.receiverToken)
                       .Return((sender, receiver) => new
                       {
                           sender = sender.As<Recipe>(),
                           receiver = receiver.As<User>()
                       })
                   .ResultsAsync;

                    users = query.ToList();
                }
                else // crea la relacion
                {
                    query = await _client.Cypher
                       .Match("(sender:User),(receiver:User)")
                       .Where("sender.token = $senderToken")
                       .AndWhere("receiver.token = $receiverToken")
                       .Create("(sender)-[follow:Following{dateCreated:$dateCreated}]->(receiver)")
                       .WithParam("senderToken", followUserRequest.senderToken)
                       .WithParam("receiverToken", followUserRequest.receiverToken)
                       .WithParam("dateCreated", today)
                       .Return((sender, receiver) => new
                       {
                           sender = sender.As<Recipe>(),
                           receiver = receiver.As<User>()

                       })
                   .ResultsAsync;

                    users = query.ToList();
                }

                return Ok(users);
            } 
            catch(Exception ex)
            {
                return BadRequest(ex);
            }
        }

        //comentar en un usuario
        [HttpPost("/user/comment_user")]
        public async Task<IActionResult> PostCommentUser([FromBody] CommentUserRequest request)
        {
            try
            {
                DateTime today = DateTime.Now;

                await _client.Cypher
                    .Match("(u1:User),(u2:User)")
                    .Where("u1.token = $senderId")
                    .AndWhere("u2.token = $receiverId")
                    .Create("(u1)-[c:Commented{comment: $comment, dateCreated: $todayDate}]->(u2)")
                    .WithParam("senderId", request.SenderId)
                    .WithParam("receiverId", request.ReceiverId)
                    .WithParam("comment", request.Comment)
                    .WithParam("todayDate", today)
                .ExecuteWithoutResultsAsync();

                return Ok();
            } 
            catch(Exception ex)
            {
                return BadRequest(ex);
            }
        }

        //devolver el contador de recetas creadas
        [HttpGet("/user/recipes_created/{token}")]
        public async Task<IActionResult> GetCountRecipes(string token)
        {
            try
            {
                var query = await _client.Cypher
                    .Match("(u:User)-[count:Created]-(r:Recipe)")
                    .Where("u.token = $token")
                    .WithParam("token", token)
                    .Return(count => count.Count())
                .ResultsAsync;

                return Ok(query);
            } 
            catch(Exception ex)
            {
                return BadRequest(ex);
            } 
        }

        //devolver el contador de followings
        [HttpGet("/user/following/{token}")]
        public async Task<IActionResult> GetCountFollowing(string token)
        {
            try
            {
                var result = await _client.Cypher
                    .Match("(u:User)-[count:Following]->(u2:User)")
                    .Where("u.token = $token")
                    .WithParam("token", token)
                    .Return(u2 => u2.Count())
                .ResultsAsync;

                return Ok(result);
            }
            catch(Exception ex)
            {
                return BadRequest(ex);
            }
        }

        //devolver el contador de followers
        [HttpGet("/user/followers/{token}")]
        public async Task<IActionResult> GetCountFollowers(string token)
        {
            var result = await _client.Cypher
                .Match("(u:User)<-[count:Following]-(u2:User)")
                .Where("u.token = $token")
                .WithParam("token", token)
                .Return(u2 => u2.Count())
            .ResultsAsync;

            return Ok(result);
        }

        //devolver el contador de likes
        [HttpGet("/user/recipes_liked/{token}")]
        public async Task<IActionResult> GetCountLiked(string token)
        {
            try
            {
                var result = await _client.Cypher
                    .Match("(u:User)-[count:Liked]->(r:Recipe)")
                    .Where("u.token = $token")
                    .WithParam("token", token)
                    .Return(r => r.Count())
                .ResultsAsync;

                return Ok(result);
            }
            catch(Exception ex)
            {
                return BadRequest(ex);
            }
        }

        //devolver comentarios de un usuario
        [HttpGet("/user/comments/{token}/{skipper}")]
        public async Task<IActionResult> GetCommentsOnUser(string token, int skipper)
        {
            try
            {
                var query = await _client.Cypher
                    .Match("(user:User)-[comment:Commented]->(u2:User)")
                    .Where("u2.token = $token")
                    .WithParam("token", token)
                    .Return((user, comment) => new
                    {
                        user = user.As<User>(),
                        comment = comment.As<CommentOnUser>()
                    })
                    .OrderBy("comment.dateCreated desc")
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

        //borrar un usuario
        [HttpPost("/user/delete")]
        public async Task<IActionResult> PostDeleteUser([FromBody] DeleteUserRequest request)
        {
            try
            {
                await _client.Cypher
                    .Match("(u:User)")
                    .Where("u.token = $token")
                    .OptionalMatch("(u)-[c:Created]-(r:Recipe)")
                    .WithParam("token", request.token)
                    .DetachDelete("u, r, c")
                .ExecuteWithoutResultsAsync();

                return Ok();
            }
            catch (Exception ex) { return BadRequest(ex); }
        }

    }
}
