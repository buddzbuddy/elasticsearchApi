using elasticsearchApi.Models;
using elasticsearchApi.Models.Infrastructure;
using elasticsearchApi.Models.Person;

namespace elasticsearchApi.Contracts.Person
{
    public interface IAddNewPersonFacade
    {
        IServiceContext AddNewPerson(addNewPersonDTO dto, in int regionNo, in int districtNo);
    }
}
