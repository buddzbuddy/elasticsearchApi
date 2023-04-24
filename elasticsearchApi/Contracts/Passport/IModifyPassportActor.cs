using elasticsearchApi.Models;
using elasticsearchApi.Services.Passport;
using System.Data;

namespace elasticsearchApi.Contracts.Passport
{
    public interface IModifyPassportActor
    {
        IServiceContext CallModifyPassport(string iin, modifyPersonPassportDTO person);
    }
}
