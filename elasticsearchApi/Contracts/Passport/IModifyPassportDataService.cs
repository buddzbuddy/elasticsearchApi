using elasticsearchApi.Models;
using elasticsearchApi.Models.Contracts;
using elasticsearchApi.Models.Passport;
using elasticsearchApi.Models.Person;
using System.Data;

namespace elasticsearchApi.Contracts.Passport
{
    public interface IModifyPassportDataService
    {
        void Execute(string iin, modifyPersonPassportDTO person, outPersonDTO? personFullData = null);
    }
}
