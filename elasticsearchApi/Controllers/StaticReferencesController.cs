using elasticsearchApi.Models;
using elasticsearchApi.Utils;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace elasticsearchApi.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class StaticReferencesController : ControllerBase
    {
        [HttpGet]
        public IActionResult Genders()
        {
            return Ok(StaticReferences.getEnumItems<Genders>());
        }
        [HttpGet]
        public IActionResult FamilyStates()
        {
            return Ok(StaticReferences.getEnumItems<FamilyStates>());
        }
        [HttpGet]
        public IActionResult PassportTypes()
        {
            return Ok(StaticReferences.getEnumItems<PassportTypes>());
        }
    }
}
