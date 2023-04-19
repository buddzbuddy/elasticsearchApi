using elasticsearchApi.Models;

namespace elasticsearchApi.Contracts.Passport
{
    public interface IPassportVerifierBasic
    {
        void Verify(modifyPersonPassportDTO passport);
    }
}
