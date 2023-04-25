using elasticsearchApi.Contracts.Infrastructure;
using elasticsearchApi.Contracts.Person;
using elasticsearchApi.Models;
using elasticsearchApi.Models.Person;

namespace elasticsearchApi.Services.Person
{
    public class AddNewPersonDataServiceImpl : IAddNewPersonDataService
    {
        private readonly IAddNewPersonVerifier _personVerifier;
        private readonly IAddressRefsVerifier _addressRefsVerifier;
        public AddNewPersonDataServiceImpl(IAddNewPersonVerifier personVerifier, IAddressRefsVerifier addressRefsVerifier)
        {
            _personVerifier = personVerifier;
            _addressRefsVerifier = addressRefsVerifier;
        }
        public void AddNewPerson(addNewPersonDTO dto, int regionNo, int districtNo)
        {
            _personVerifier.VerifyPerson(dto);
            _addressRefsVerifier.Verify(regionNo, districtNo);


        }
    }
}
