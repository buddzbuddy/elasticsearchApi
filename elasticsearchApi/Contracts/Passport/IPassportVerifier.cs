using elasticsearchApi.Models;
using System.Data;

namespace elasticsearchApi.Contracts.Passport
{
    public interface IPassportVerifier
    {
        void VerifyPassport(modifyPersonPassportDTO passport, IDbTransaction? transaction = null);
    }
}
