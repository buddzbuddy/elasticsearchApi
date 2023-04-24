using elasticsearchApi.Models;
using elasticsearchApi.Models.Person;

namespace elasticsearchApi.Contracts.Person
{
    public interface IAddNewPersonVerifier
    {
        void VerifyPerson(addNewPersonDTO dto);
    }
}
