using elasticsearchApi.Models;

namespace elasticsearchApi.Contracts
{
    public interface IPassportVerifier
    {
        void VerifyPassport(modifyPersonPassportDTO passport);
    }
}
