using elasticsearchApi.Models;

namespace elasticsearchApi.Contracts
{
    public interface IPassportVerifierLogic
    {
        void Verify(modifyPersonPassportDTO passport);
    }
}
