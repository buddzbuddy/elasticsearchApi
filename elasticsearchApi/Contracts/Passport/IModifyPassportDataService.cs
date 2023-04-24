using elasticsearchApi.Models;
using elasticsearchApi.Models.Passport;
using System.Data;

namespace elasticsearchApi.Contracts.Passport
{
    public interface IModifyPassportDataService
    {
        void Execute(string iin, modifyPersonPassportDTO person);
    }
}
