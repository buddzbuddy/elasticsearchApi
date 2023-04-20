using elasticsearchApi.Models;
using System.Data;

namespace elasticsearchApi.Contracts.Passport
{
    public interface IModifyPassportActor
    {
        IServiceContext CallModifyPassport(string iin, modifyPersonPassportDTO person, IDbTransaction? transaction = null);
        IServiceContext CallModifyPassport(string iin, modifyPersonPassportDTO person, ref IDbTransaction? transaction);
    }
}
