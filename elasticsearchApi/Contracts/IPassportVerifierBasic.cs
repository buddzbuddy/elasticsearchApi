using elasticsearchApi.Models;

namespace elasticsearchApi.Contracts
{
    public interface IPassportVerifierBasic
    {
        void Verify(modifyPersonPassportDTO passport);
    }
}
