using elasticsearchApi.Models;
using elasticsearchApi.Models.Contracts;
using elasticsearchApi.Models.Passport;

namespace elasticsearchApi.Contracts.Passport
{
    public interface IPassportVerifierBasic
    {
        void Verify(IPassportData passport);
    }
}
