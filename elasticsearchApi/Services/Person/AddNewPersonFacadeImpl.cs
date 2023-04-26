using elasticsearchApi.Contracts.CheckProviders;
using elasticsearchApi.Contracts.Infrastructure;
using elasticsearchApi.Contracts.Person;
using elasticsearchApi.Models;
using elasticsearchApi.Models.Infrastructure;
using elasticsearchApi.Models.Person;
using Nest;

namespace elasticsearchApi.Services.Person
{
    public class AddNewPersonFacadeImpl : IAddNewPersonFacade
    {
        private readonly IAddNewPersonVerifier _personVerifier;
        private readonly IAddressRefsVerifier _addressRefsVerifier;
        private readonly ICheckFacade _checkFacade;
        public AddNewPersonFacadeImpl(IAddNewPersonVerifier personVerifier, IAddressRefsVerifier addressRefsVerifier, ICheckFacade checkFacade)
        {
            _personVerifier = personVerifier;
            _addressRefsVerifier = addressRefsVerifier;
            _checkFacade = checkFacade;
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



            return context;
        }
    }
}
