using elasticsearchApi.Models.Person;

namespace elasticsearchApi.Contracts.Person
{
    public interface IPersonCreator
    {
        int CreateNewPerson(addNewPersonDTO dto, string newIin);
    }
}
