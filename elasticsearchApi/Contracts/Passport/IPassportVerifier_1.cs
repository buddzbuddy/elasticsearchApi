using elasticsearchApi.Models;
using elasticsearchApi.Models.Contracts;
using elasticsearchApi.Models.Passport;

namespace elasticsearchApi.Contracts.Passport
{
    public interface IPassportVerifier
    {
        void Verify(IPassportData passport);
    }
}
