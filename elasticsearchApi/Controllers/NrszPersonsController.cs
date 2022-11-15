using AutoMapper;
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
        private readonly NrszService svc;
        private readonly IOptions<AppSettings> _appSettings;
        private readonly IMapper _mapper;
        public NrszPersonsController(IOptions<AppSettings> appSettings, IMapper mapper)
        {
            _appSettings = appSettings;
            _mapper = mapper;
            svc = new (_appSettings.Value, mapper);
        }
        [HttpGet]
        public IActionResult SomePerson()
        {
            try
            {
                var result = svc.SomePersons();
                return Ok(result);
            }
            catch (Exception e)
            {
                return BadRequest(e.GetBaseException().Message);
            }
        }
        [HttpGet]
        public IActionResult RegCounters()
        {
            try
            {
                return Ok(FakeDb.RegCounters);
            }
            catch (Exception e)
            {
                return BadRequest(e.GetBaseException().Message);
            }
        }
        [HttpPost]
        public IActionResult IndexDoc([FromBody] SearchPersonModel person)
        {
            try
            {
                var settings = new ConnectionSettings(new Uri(_appSettings.Value.host)).DefaultIndex("nrsz_persons2");
                var client = new ElasticClient(settings);
                client.Index(person, i => i.Refresh(Refresh.True));
                return Ok();
            }
            catch (Exception e)
            {
                return BadRequest(e.GetBaseException().Message);
            }
        }
        [HttpGet]
        public IActionResult IncreaseTest(int regCode)
        {
            try
            {
                FakeDb.Increase(regCode);
                return Ok();
            }
            catch (Exception e)
            {
                return BadRequest(e.GetBaseException().Message);
            }
        }
        [HttpPost]
        public IActionResult FindSamePerson([FromBody] SearchPersonModel person)
        {
            try
            {
                var result = svc.FindSamePersonES(person);
                return Ok(result);
            }
            catch (Exception e)
            {
                return BadRequest(e.GetBaseException().Message);
            }
        }
        [HttpPost]
        public IActionResult FindSamePersonOld([FromBody] Person person)
        {
            try
            {
                var result = svc.FindSamePerson(person);
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
                var result = svc.FindPersonsES(person);
                return Ok(result);
            }
            catch (Exception e)
            {
                return BadRequest(e.GetBaseException().Message);
            }
        }
        [HttpPost]
        public IActionResult FindPersonsOld([FromBody] Person person)
        {
            try
            {
                var result = svc.FindPersons(person);
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
                var result = svc.FindPersonByPINES(nrszPerson);
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
                var result = svc.AddNewPerson(nrszPerson, regionNo, districtNo);
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
                //svc.InitRegionDistricts();
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
