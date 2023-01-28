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
        public NrszPersonsController(IOptions<AppSettings> appSettings, IMapper mapper, ElasticClient client)
        {
            _appSettings = appSettings;
            _mapper = mapper;
            es = new(_appSettings.Value.es_host, _appSettings.Value.nrsz_persons_index_name, _appSettings.Value.log_enabled, _appSettings.Value.logpath, _mapper, appSettings.Value, client);
        }
        
        [HttpPost]
        public IActionResult FindSamePerson([FromBody] SearchPersonModel person, int page = 1, int size = 10)
        {
            ServiceContext context = new();
            try
            {
                es.FindSamePersonES(person, ref context, page, size);
                return Ok(context);
            }
            catch (Exception e)
            {
                context.SuccessFlag = false;
                context.AddErrorMessage("errorMessage", e.GetBaseException().Message);
                context.AddErrorMessage("errorTrace", e.StackTrace);
                return Ok(context);
            }
        }
        [HttpPost]
        public IActionResult FindPersons([FromBody] SearchPersonModel person, int page = 1, int size = 10, bool fuzzy = false)
        {
            ServiceContext context = new();
            try
            {
                es.FindPersonsES(person, ref context, fuzzy, page, size);
                return Ok(context);
            }
            catch (Exception e)
            {
                context.SuccessFlag = false;
                context.AddErrorMessage("errorMessage", e.GetBaseException().Message);
                context.AddErrorMessage("errorTrace", e.StackTrace);
                return Ok(context);
            }
        }
        [HttpGet("{iin}")]
        public IActionResult FindPersonByPIN(string iin, int page = 1, int size = 10)
        {
            ServiceContext context = new();
            try
            {
                es.FindPersonByPinES(iin, ref context, page, size);
                return Ok(context);
            }
            catch (Exception e)
            {
                context.SuccessFlag = false;
                context.AddErrorMessage("errorMessage", e.GetBaseException().Message);
                context.AddErrorMessage("errorTrace", e.StackTrace);
                return Ok(context);
            }
        }
        [HttpPost]
        public IActionResult Filter([FromBody] IDictionary<string, object> filter, int page = 1, int size = 10, bool fuzzy = false)
        {
            try
            {
                var result = es.FilterES(filter, out personDTO[] data, out string[] errorMessages, fuzzy, page, size);
                return Ok(new { successFlag = result, data, errorMessages });
            }
            catch (Exception e)
            {
                return Ok(new { result = false, errorMessages = new[] { e.GetBaseException().Message }, trace = e.StackTrace });
            }
        }
        [HttpPost]
        public IActionResult FilterDocumentES([FromBody] documentDTO filter, int page = 1, int size = 10, bool fuzzy = false)
        {
            try
            {
                var result = es.FilterDocumentES(filter, out IEnumerable<documentDTO> data, out string[] errorMessages);
                return Ok(new { result, data, errorMessages });
            }
            catch (Exception e)
            {
                return Ok(new { result = false, errorMessages = new[] { e.GetBaseException().Message }, trace = e.StackTrace });
            }
        }
    }
}
