using elasticsearchApi.Models.Contracts;
using elasticsearchApi.Models.Person;

namespace elasticsearchApi.Contracts.Person
{
    public interface IPersonBasicVerifier
    {
        void Verify(IPersonData dto);
    }
}
