using elasticsearchApi.Contracts.CheckProviders;
using elasticsearchApi.Contracts.Infrastructure;
using elasticsearchApi.Contracts.Person;
using elasticsearchApi.Models;
using elasticsearchApi.Models.Exceptions.PIN;
using elasticsearchApi.Models.Infrastructure;
using elasticsearchApi.Models.Person;
using elasticsearchApi.Utils;
using Nest;
using System;

namespace elasticsearchApi.Services.Person
{
    public class AddNewPersonFacadeImpl : IAddNewPersonFacade
    {
        private readonly IAddNewPersonVerifier _personVerifier;
        private readonly IAddressRefsVerifier _addressRefsVerifier;
        private readonly ICheckFacade _checkFacade;
        private readonly ICreatePersonFacade _createPersonFacade;
        public AddNewPersonFacadeImpl(IAddNewPersonVerifier personVerifier, IAddressRefsVerifier addressRefsVerifier,
            ICheckFacade checkFacade, ICreatePersonFacade createPersonFacade)
        {
            _personVerifier = personVerifier;
            _addressRefsVerifier = addressRefsVerifier;
            _checkFacade = checkFacade;
            _createPersonFacade = createPersonFacade;
        }
        public IServiceContext AddNewPerson(addNewPersonDTO dto, in int regionNo, in int districtNo)
        {
            var context = new ServiceContext();
            _personVerifier.VerifyPerson(dto);
            _addressRefsVerifier.Verify(regionNo, districtNo);

            var existingPerson = _checkFacade.CallCheck(BaseService.ModelToDict(dto));
            if(existingPerson != null)
            {
                context["NewPIN"] = existingPerson.iin ?? "";
                context["Result"] = existingPerson;
                context["ResultPIN"] = existingPerson.iin ?? "";
                context.SuccessFlag = true;
                return context;
            }

            var newPerson = _createPersonFacade.CreateNewPerson(dto, regionNo, districtNo);
            var newPin = newPerson.iin;
            if (string.IsNullOrEmpty(newPin))
                throw new PinNotGeneratedException("Пин не сгенерировался");

            context["NewPIN"] = newPin;
            context["Result"] = newPerson;
            context["ResultPIN"] = newPin;

            return context;
        }
    }
}
