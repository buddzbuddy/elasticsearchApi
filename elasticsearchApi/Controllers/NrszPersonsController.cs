using Elasticsearch.Net;
using elasticsearchApi.Models;
using elasticsearchApi.Utils;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Nest;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace elasticsearchApi.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class NrszPersonsController : ControllerBase
    {
        private NewScriptExecutor executor;
        private readonly IOptions<AppSettings> _appSettings;
        public NrszPersonsController(IOptions<AppSettings> appSettings)
        {
            _appSettings = appSettings;
            executor = new (_appSettings.Value);
        }
        [HttpPost]
        public IActionResult FindSamePerson([FromBody] SearchPersonModel person)
        {
            try
            {
                var result = executor.FindSamePerson2(person);
                return Ok(result);
            }
            catch (Exception e)
            {
                return BadRequest(e.GetBaseException().Message);
            }
        }
        [HttpPost]
        public IActionResult FindPersons([FromBody] SearchPersonModel person)
        {
            try
            {
                var result = executor.FindPersons2(person);
                return Ok(result);
            }
            catch (Exception e)
            {
                return BadRequest(e.GetBaseException().Message);
            }
        }
        [HttpPost]
        public IActionResult FindPersonByPIN([FromBody] SearchPersonModel nrszPerson)
        {
            try
            {
                var result = executor.FindPersonByPIN2(nrszPerson);
                return Ok(result);
            }
            catch (Exception e)
            {
                return BadRequest(e.GetBaseException().Message);
            }
        }
        [HttpPost]
        public IActionResult AddNewPerson(int regionNo, int districtNo, [FromBody] Person nrszPerson)
        {
            try
            {
                var result = executor.AddNewPerson(nrszPerson, regionNo, districtNo);
                return Ok(result);
            }
            catch (Exception e)
            {
                return BadRequest(e.GetBaseException().Message + ", trace: " + e.StackTrace);
            }
        }

        [HttpGet]
        public IActionResult InitRegionDistricts()
        {
            try
            {
                executor.InitRegionDistricts();
                return Ok(Refs.RegionDistricts);
            }
            catch (Exception e)
            {
                return BadRequest(e.GetBaseException().Message + ", trace: " + e.StackTrace);
            }
        }

        [HttpGet]
        public IActionResult DeleteTestPerson(int tempId)
        {
            try
            {
                //executor.DeleteTestPerson(tempId);
                return Ok();
            }
            catch (Exception e)
            {
                return BadRequest(e.GetBaseException().Message + ", trace: " + e.StackTrace);
            }
        }

        static void WriteLog(object text)
        {
            using (StreamWriter sw = new("c:\\distr\\cissa\\nrsz-rest-api.log", true))
            {
                sw.WriteLine(text.ToString());
            }
        }
    }
}
