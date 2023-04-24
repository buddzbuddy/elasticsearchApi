using elasticsearchApi.Models;
using elasticsearchApi.Models.Person;

namespace elasticsearchApi.Contracts.Person
{
    public interface IAddNewPersonDataService
    {
        void AddNewPerson(addNewPersonDTO dto);
    }
}
