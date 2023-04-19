using elasticsearchApi.Models;

namespace elasticsearchApi.Contracts.Passport
{
    public interface IModifyPassportActor
    {
        IServiceContext CallModifyPassport(string iin, modifyPersonPassportDTO person);
    }
}
