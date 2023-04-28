using AutoMapper;
using elasticsearchApi.Contracts;
using elasticsearchApi.Contracts.Passport;
using elasticsearchApi.Contracts.Person;
using elasticsearchApi.Models;
using elasticsearchApi.Models.Infrastructure;
using elasticsearchApi.Models.Passport;
using elasticsearchApi.Models.Person;
using elasticsearchApi.Services;
using elasticsearchApi.Utils;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Nest;
using System;
using System.Collections.Generic;

namespace elasticsearchApi.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class NrszPersonsController : ControllerBase
    {
        private readonly IElasticService _es;
        private readonly IDataService _dataSvc;
        private IServiceContext _context;
        private readonly IModifyPassportActor _modifyPassportActor;
        private readonly IAddNewPersonFacade _addNewPersonFacade;
        public NrszPersonsController(IElasticService es, IDataService dataSvc, IServiceContext context,
            IModifyPassportActor modifyPassportActor, IAddNewPersonFacade addNewPersonFacade)
        {
            _es = es;
            _dataSvc = dataSvc;
            _context = context;
            _modifyPassportActor = modifyPassportActor;
            _addNewPersonFacade = addNewPersonFacade;
        }
        
        [HttpPost]
        public IActionResult FindSamePerson([FromBody] inputPersonDTO person, int page = 1, int size = 10)
        {
            try
            {
                _es.FindSamePersonES(person, ref _context, page, size);
                return Ok(_context);
            }
            catch (Exception e)
            {
                _context.SuccessFlag = false;
                _context.AddErrorMessage("errorMessage", e.GetBaseException().Message);
                _context.AddErrorMessage("errorTrace", e.StackTrace ?? "");
                return Ok(_context);
            }
        }
        [HttpPost]
        public IActionResult FindPersons([FromBody] inputPersonDTO person, int page = 1, int size = 10, bool fuzzy = false)
        {
            try
            {
                _es.FindPersonsES(person, ref _context, fuzzy, page, size);
                return Ok(_context);
            }
            catch (Exception e)
            {
                _context.SuccessFlag = false;
                _context.AddErrorMessage("errorMessage", e.GetBaseException().Message);
                _context.AddErrorMessage("errorTrace", e.StackTrace ?? "");
                return Ok(_context);
            }
        }
        [HttpGet("{iin}")]
        public IActionResult FindPersonByPIN(string iin, int page = 1, int size = 10)
        {
            try
            {
                _es.FindPersonByPinES(iin, ref _context, page, size);
                return Ok(_context);
            }
            catch (Exception e)
            {
                _context.SuccessFlag = false;
                _context.AddErrorMessage("errorMessage", e.GetBaseException().Message);
                _context.AddErrorMessage("errorTrace", e.StackTrace ?? "");
                return Ok(_context);
            }
        }
        [HttpPost]
        public IActionResult Filter([FromBody] IDictionary<string, object> filter, int page = 1, int size = 10, bool fuzzy = false)
        {
            try
            {
                var result = _es.FilterES(filter, out outPersonDTO[] data, out string[] errorMessages, out long totalCount, fuzzy, page, size);
                return Ok(new { successFlag = result, data, totalCount, errorMessages });
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
                var result = _es.FilterDocumentES(filter, out IEnumerable<documentDTO> data, out string[] errorMessages);
                return Ok(new { result, data, errorMessages });
            }
            catch (Exception e)
            {
                return Ok(new { result = false, errorMessages = new[] { e.GetBaseException().Message }, trace = e.StackTrace });
            }
        }
        [HttpPost]
        public IActionResult AddNewPerson([FromBody] addNewPersonDTO person, int regionNo, int districtNo)
        {
            try
            {
                _dataSvc.AddNewPerson(person, regionNo, districtNo, ref _context);
                return Ok(_context);
            }
            catch (Exception e)
            {
                _context.SuccessFlag = false;
                _context.AddErrorMessage("errorMessage", e.GetBaseException().Message);
                _context.AddErrorMessage("errorTrace", e.StackTrace ?? "");
                return Ok(_context);
            }
        }
        [HttpPost]
        public IActionResult AddNewPersonV2([FromBody] addNewPersonDTO person, int regionNo, int districtNo)
        {
            _context.SuccessFlag = false;
            try
            {
                return Ok(_addNewPersonFacade.AddNewPerson(person, regionNo, districtNo));
            }
            catch (Exception e)
            {
                _context.AddErrorMessage("errorMessage", e.GetBaseException().Message);
                _context.AddErrorMessage("errorTrace", e.StackTrace ?? "");
                return Ok(_context);
            }
        }
        [HttpPost]
        public IActionResult AddNewPersonFake([FromBody] addNewPersonDTO person, int regionNo, int districtNo)
        {
            try
            {
                //_dataSvc.AddNewPerson(person, regionNo, districtNo, ref _context);
                var regCode = regionNo * 1000 + districtNo;
                var pin = regCode * 10000000000;
                DateTimeOffset now = DateTimeOffset.UtcNow;
                long unixTimeMilliseconds = now.ToUnixTimeMilliseconds();
                pin += unixTimeMilliseconds;
                person.iin = pin.ToString();

                _context["NewPIN"] = pin;
                _context.SuccessFlag = true;
                _context["Result"] = person;
                _context["ResultPIN"] = pin;

                return Ok(_context);
            }
            catch (Exception e)
            {
                _context.SuccessFlag = false;
                _context.AddErrorMessage("errorMessage", e.GetBaseException().Message);
                _context.AddErrorMessage("errorTrace", e.StackTrace ?? "");
                return Ok(_context);
            }
        }
        [HttpPost]
        public IActionResult ModifyPersonData(string iin, [FromBody] modifyPersonDataDTO person)
        {
            try
            {
                _dataSvc.ModifyPersonData(iin, person, ref _context);
                return Ok(_context);
            }
            catch (Exception e)
            {
                _context.SuccessFlag = false;
                _context.AddErrorMessage("errorMessage", e.GetBaseException().Message);
                _context.AddErrorMessage("errorTrace", e.StackTrace ?? "");
                return Ok(_context);
            }
        }
        [HttpPost]
        public IActionResult ModifyPersonPassport(string iin, [FromBody] modifyPersonPassportDTO person)
        {
            _context.SuccessFlag = false;
            try
            {
                _context = _modifyPassportActor.CallModifyPassport(iin, person);
            }
            catch (Exception e)
            {
                _context.AddErrorMessage("errorMessage", e.GetBaseException().Message);
                _context.AddErrorMessage("type", "ModifyPassportActor - Выполнено необработанное исключение");
                _context.AddErrorMessage("errorTrace", e.StackTrace ?? "");
            }
            return Ok(_context);
        }
    }
}
