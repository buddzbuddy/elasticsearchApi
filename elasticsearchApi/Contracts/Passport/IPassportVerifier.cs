using elasticsearchApi.Models;

namespace elasticsearchApi.Contracts.Passport
{
    public interface IPassportVerifier
    {
        void VerifyPassport(modifyPersonPassportDTO passport);
    }
}
