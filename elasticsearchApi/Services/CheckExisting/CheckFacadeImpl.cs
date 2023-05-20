using elasticsearchApi.Contracts;
using elasticsearchApi.Contracts.CheckProviders;
using elasticsearchApi.Contracts.DataProviders;
using elasticsearchApi.Contracts.Delegates;
using elasticsearchApi.Models.Person;
using elasticsearchApi.Services.CheckExisting.Providers;

namespace elasticsearchApi.Services.CheckExisting
{
    public class CheckFacadeImpl : ICheckFacade
    {
        private readonly ICheckService _checkService;
        private readonly ExistingPassportVerifierResolver _existingPassportVerifierResolver;
        public CheckFacadeImpl(ICheckService checkService, ExistingPassportVerifierResolver existingPassportVerifierResolver)
        {
            _checkService = checkService;
            _existingPassportVerifierResolver= existingPassportVerifierResolver;
        }
        public outPersonDTO? CallCheck(IDictionary<string, object?> filter, IDictionary<string, object?>? excludeFilter = null)
        {
            var data = _checkService.CheckExisting(_existingPassportVerifierResolver("Memory"), filter, excludeFilter);
            data ??= _checkService.CheckExisting(_existingPassportVerifierResolver("Elastic"), filter, excludeFilter);
            data ??= _checkService.CheckExisting(_existingPassportVerifierResolver("Database"), filter, excludeFilter);
            return data;
        }
    }
}
