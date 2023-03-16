using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace elasticsearchApi.Controllers
{
    [Route("[controller]")]
    [ApiController]
    [AllowAnonymous]
    public class SettingsController : ControllerBase
    {
        private readonly IConfiguration _config;

        public SettingsController(IConfiguration config)
        {
            _config = config;
        }

        [HttpGet("mysetting")]
        public ActionResult<string> GetSetting()
        {
            return Ok(_config["MySetting"]);
        }
    }
}
