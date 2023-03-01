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
        public UsersController(IUserService userService) {
            _userService = userService;
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
