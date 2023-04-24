using elasticsearchApi.Models;
using elasticsearchApi.Models.Passport;

namespace elasticsearchApi.Contracts.Passport
{
    public interface IPassportVerifierLogic
    {
        void Verify(modifyPersonPassportDTO passport);
    }
}
