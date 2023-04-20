using elasticsearchApi.Models;
using elasticsearchApi.Services.Passport;
using System.Data;

namespace elasticsearchApi.Contracts.Passport
{
    public interface IModifyPassportActor
    {
        IServiceContext CallModifyPassport(string iin, modifyPersonPassportDTO person, IDbTransaction? transaction = null);
        IServiceContext CallModifyPassport(string iin, modifyPersonPassportDTO person, ref IDbTransaction? transaction);
        IServiceContext CallModifyPassport(string iin, modifyPersonPassportDTO person, OnSuccess? onSuccessFlag, OnFailure? onFailure, ref IDbTransaction? transaction);
    }
}
