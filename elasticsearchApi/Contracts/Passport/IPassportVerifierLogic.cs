using elasticsearchApi.Models;

namespace elasticsearchApi.Contracts.Passport
{
    public interface IPassportVerifierLogic
    {
        void Verify(modifyPersonPassportDTO passport);
    }
}
