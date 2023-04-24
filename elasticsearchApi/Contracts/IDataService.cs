using elasticsearchApi.Models.Infrastructure;
using elasticsearchApi.Models.Person;

namespace elasticsearchApi.Contracts
{
    public interface IDataService
    {
        void AddNewPerson(addNewPersonDTO person, int regionNo, int districtNo, ref IServiceContext context);
        void ModifyPersonData(string iin, modifyPersonDataDTO person, ref IServiceContext context);
    }
}
