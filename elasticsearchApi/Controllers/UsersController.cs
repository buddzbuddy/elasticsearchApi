using elasticsearchApi.Data.Entities;
using elasticsearchApi.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Linq;
using System.Threading.Tasks;

namespace elasticsearchApi.Controllers
{
    [Route("api/[controller]")]
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
    }
}
