using elasticsearchApi.Contracts;
using elasticsearchApi.Contracts.CheckProviders;
using elasticsearchApi.Contracts.DataProviders;
using elasticsearchApi.Models.Person;
using elasticsearchApi.Services.CheckExisting.Providers;

namespace elasticsearchApi.Services.CheckExisting
{
    public class CheckFacadeImpl : ICheckFacade
    {
        private readonly ICheckService _checkService;
        private readonly CheckProviderMemoryImpl _checkProviderMemory;
        private readonly CheckProviderElasticImpl _checkProviderElastic;
        public CheckFacadeImpl(ICheckService checkService, CheckProviderMemoryImpl checkProviderMemory,
            CheckProviderElasticImpl checkProviderElastic)
        {
            _checkService = checkService;
            _checkProviderMemory = checkProviderMemory;
            _checkProviderElastic = checkProviderElastic;
        }
        public outPersonDTO? CallCheck(IDictionary<string, object?> filter, IDictionary<string, object?>? excludeFilter = null)
        {
            var data = _checkService.CheckExisting(_checkProviderMemory, filter, excludeFilter);
            if(data == null)
            {
                data = _checkService.CheckExisting(_checkProviderElastic, filter, excludeFilter);
            }
            return data;
        }
    }
}
