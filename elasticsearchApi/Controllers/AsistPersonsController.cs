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
    public class AsistPersonsController : ControllerBase
    {
        private readonly AsistService svc;
        private readonly IOptions<AppSettings> _appSettings;
        private readonly IMapper _mapper;
        public AsistPersonsController(IOptions<AppSettings> appSettings, IMapper mapper)
        {
            _appSettings = appSettings;
            _mapper = mapper;
            svc = new (_appSettings.Value);
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
        [HttpPost]
        public IActionResult IndexDoc([FromBody] SearchPersonModel person)
        {
            try
            {
                var settings = new ConnectionSettings(new Uri(_appSettings.Value.host)).DefaultIndex("asist_persons2");
                var client = new ElasticClient(settings);
                client.Index(person, i => i.Refresh(Refresh.True));
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
        public IActionResult Filter([FromBody] IDictionary<string, string> filter)
        {
            try
            {
                var result = svc.FilterES(filter, out personDTO[] data, out string[] errorMessages);
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
                var result = svc.FilterDocumentES(filter, out IEnumerable<documentDTO> data, out string[] errorMessages);
                return Ok(new { result, data, errorMessages });
            }
            catch (Exception e)
            {
                return Ok(new { result = false, errorMessages = new[] { e.GetBaseException().Message } });
            }
        }
        
        
        [HttpPost]
        public IActionResult Fuzzy([FromBody] IDictionary<string, string> filter)
        {
            try
            {
                var result = svc.Fuzzy(filter, out personDTO[] data, out string[] errorMessages);
                return Ok(new { result, data, errorMessages });
            }
            catch (Exception e)
            {
                return Ok(new { result = false, errorMessages = new[] { e.GetBaseException().Message } });
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
    }
}
