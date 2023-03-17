using elasticsearchApi.Data.Entities;
using elasticsearchApi.Models;
using elasticsearchApi.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Linq;
using System.Threading.Tasks;

namespace elasticsearchApi.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly IUserService _userService;
        private readonly IUsers _users;
        public UsersController(IUserService userService, IUsers users)
        {
            _userService = userService;
            _users = users;
        }

        [HttpGet()]
        public async Task<ActionResult<User>> GetUsers()
        {
            return Ok(await _users.All());
        }

        [HttpGet("GetUsers")]
        public async Task<IActionResult> Get()
        {
            var users = await _userService.GetAllMyUsers();
            if (users.Any())
                return Ok(users);
            return NotFound();
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<User>> GetUserById(int id)
        {
            var user = await _users.WithId(id);
            return user != null ? Ok(user) : NotFound();
        }

        [HttpPut("")]
        public async Task<ActionResult<User>> AddUser(AddUserModel model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            var user = await _users.Add(model.FirstName!, model.LastName!);
            return CreatedAtAction("GetUserById", new { id = user.Id }, user);
        }
    }
}
