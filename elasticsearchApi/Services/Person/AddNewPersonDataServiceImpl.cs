using elasticsearchApi.Contracts.Person;
using elasticsearchApi.Models;
using elasticsearchApi.Models.Person;

namespace elasticsearchApi.Services.Person
{
    public class AddNewPersonDataServiceImpl : IAddNewPersonDataService
    {
        private readonly IAddNewPersonVerifier _personVerifier;
        public AddNewPersonDataServiceImpl(IAddNewPersonVerifier personVerifier) {
            _personVerifier = personVerifier;
        }
        public void AddNewPerson(addNewPersonDTO dto)
        {
            _personVerifier.VerifyPerson(dto);
            
            throw new NotImplementedException();
        }
    }
}
