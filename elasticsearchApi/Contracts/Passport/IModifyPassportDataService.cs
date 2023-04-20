using elasticsearchApi.Models;
using System.Data;

namespace elasticsearchApi.Contracts.Passport
{
    public interface IModifyPassportDataService
    {
        void Execute(string iin, modifyPersonPassportDTO person, IDbTransaction? transaction = null);
        void Execute(string iin, modifyPersonPassportDTO person, ref IDbTransaction? transaction);
    }
}
