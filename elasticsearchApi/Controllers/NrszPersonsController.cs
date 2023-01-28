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
using static elasticsearchApi.Utils.ElasticService;

namespace elasticsearchApi.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class NrszPersonsController : ControllerBase
    {
        private readonly ElasticService es;
        private readonly IOptions<AppSettings> _appSettings;
        private readonly IMapper _mapper;
        public NrszPersonsController(IOptions<AppSettings> appSettings, IMapper mapper)
        {
            _appSettings = appSettings;
            _mapper = mapper;
            es = new(_appSettings.Value.host, _appSettings.Value.nrsz_persons_index_name, _appSettings.Value.log_enabled, _appSettings.Value.logpath, _mapper, appSettings.Value);
        }
        /*[HttpGet]
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
        }*/
        [HttpPost]
        public IActionResult FindSamePerson([FromBody] SearchPersonModel person)
        {
            ServiceContext context = new();
            try
            {
                es.FindSamePersonES(person, ref context);
                return Ok(context);
            }
            catch (Exception e)
            {
                context.AddErrorMessage("", e.GetBaseException().Message);
                return Ok(context);
            }
        }
        [HttpPost]
        public IActionResult FindPersons([FromBody] SearchPersonModel person)
        {
            ServiceContext context = new();
            try
            {
                es.FindPersonsES(person, ref context);
                return Ok(context);
            }
            catch (Exception e)
            {
                context.AddErrorMessage("", e.GetBaseException().Message);
                return Ok(context);
            }
        }
        [HttpPost]
        public IActionResult FindPersonByPIN([FromBody] SearchPersonModel nrszPerson)
        {
            try
            {
                var result = es.FindPersonByPINES(nrszPerson);
                return Ok(result);
            }
            catch (Exception e)
            {
                return BadRequest(e.GetBaseException().Message);
            }
        }
        [HttpPost]
        public IActionResult Filter([FromBody] IDictionary<string, string> filter)
        {
            try
            {
                var result = es.FilterES(filter, out personDTO[] data, out string[] errorMessages);
                return Ok(new { result, data, errorMessages });
            }
            catch (Exception e)
            {
                return Ok(new { result = false, errorMessages = new[] { e.GetBaseException().Message } });
            }
        }
        [HttpPost]
        public IActionResult FilterDocumentES([FromBody] documentDTO filter)
        {
            try
            {
                var result = es.FilterDocumentES(filter, out IEnumerable<documentDTO> data, out string[] errorMessages);
                return Ok(new { result, data, errorMessages });
            }
            catch (Exception e)
            {
                return Ok(new { result = false, errorMessages = new[] { e.GetBaseException().Message } });
            }
        }
        /*[HttpPost]
        public IActionResult AddNewPerson(int regionNo, int districtNo, [FromBody] SearchPersonModel nrszPerson)
        {
            try
            {
                var result = es.AddNewPersonES(nrszPerson, regionNo, districtNo);
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
        */
        static void WriteLog(object text)
        {
            using (StreamWriter sw = new("c:\\distr\\cissa\\nrsz-rest-api.log", true))
            {
                sw.WriteLine(text.ToString());
            }
        }
    }
}
