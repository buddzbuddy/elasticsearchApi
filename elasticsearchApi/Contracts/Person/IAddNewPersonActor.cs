using elasticsearchApi.Models.Infrastructure;
using elasticsearchApi.Models.Person;

namespace elasticsearchApi.Contracts.Person
{
    public interface IAddNewPersonActor
    {
        IServiceContext CallAddNewPerson(addNewPersonDTO personDTO);
    }
}
