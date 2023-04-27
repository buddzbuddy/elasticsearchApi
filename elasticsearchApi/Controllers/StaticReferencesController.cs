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
        public IActionResult Genders()
        {
            return Ok(StaticReferences.getEnumItems<Genders>());
        }
        public IActionResult FamilyStates()
        {
            return Ok(StaticReferences.getEnumItems<FamilyStates>());
        }
        public IActionResult PassportTypes()
        {
            return Ok(StaticReferences.getEnumItems<PassportTypes>());
        }
    }
}
