using elasticsearchApi.Models.Person;

namespace elasticsearchApi.Contracts.Person
{
    public interface ICreatePersonFacade
    {
        outPersonDTO CreateNewPerson(addNewPersonDTO dto, in int regionNo, in int districtNo);
    }
}
