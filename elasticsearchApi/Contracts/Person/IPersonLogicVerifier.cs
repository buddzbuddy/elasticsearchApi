using elasticsearchApi.Models.Contracts;
using elasticsearchApi.Models.Person;

namespace elasticsearchApi.Contracts.Person
{
    public interface IPersonLogicVerifier
    {
        void Verify(IPersonData dto);
    }
}
