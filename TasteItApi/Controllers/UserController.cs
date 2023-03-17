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
        [HttpGet("/user/byname/{username}")]
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

        [HttpGet("/user/bytoken/{token}")]
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








    }
}
