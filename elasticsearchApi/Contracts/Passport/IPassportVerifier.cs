using elasticsearchApi.Models;
using elasticsearchApi.Models.Contracts;
using elasticsearchApi.Models.Passport;
using System.Data;

namespace elasticsearchApi.Contracts.Passport
{
    public interface IPassportVerifier
    {
        void VerifyPassport(IPassportData passport);
    }
}
