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
        private readonly IInMemoryProvider _inMemoryProvider;
        private readonly IElasticService _elasticService;
        public CheckFacadeImpl(ICheckService checkService, IInMemoryProvider inMemoryProvider, IElasticService elasticService)
        {
            _checkService = checkService;
            _inMemoryProvider = inMemoryProvider;
            _elasticService = elasticService;
        }
        public outPersonDTO? CallCheck(IDictionary<string, object?> filter)
        {
            var data = _checkService.CheckExisting(new CheckMemory(_inMemoryProvider), filter);
            if(data == null)
            {
                data = _checkService.CheckExisting(new CheckElastic(_elasticService), filter);
            }
            return data;
        }
    }
}
