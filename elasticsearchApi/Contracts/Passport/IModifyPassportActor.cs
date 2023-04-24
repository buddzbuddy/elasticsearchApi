using elasticsearchApi.Models.Infrastructure;
using elasticsearchApi.Models.Passport;
using elasticsearchApi.Services.Passport;
using System.Data;

namespace elasticsearchApi.Contracts.Passport
{
    public interface IModifyPassportActor
    {
        IServiceContext CallModifyPassport(string iin, modifyPersonPassportDTO person);
    }
}
