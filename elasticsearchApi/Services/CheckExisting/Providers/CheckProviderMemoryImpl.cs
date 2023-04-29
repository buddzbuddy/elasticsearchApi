using elasticsearchApi.Contracts;
using elasticsearchApi.Contracts.CheckProviders;
using elasticsearchApi.Contracts.DataProviders;
using elasticsearchApi.Models.Person;

namespace elasticsearchApi.Services.CheckExisting.Providers
{
    public class CheckProviderMemoryImpl : ICheckProvider
    {
        private readonly IInMemoryProvider _inMemorySvc;
        public CheckProviderMemoryImpl(IInMemoryProvider inMemorySvc)
        {
            _inMemorySvc = inMemorySvc;
        }
        public outPersonDTO[] FetchData(IDictionary<string, object?> filter, IDictionary<string, object?>? excludeFilter = null)
        {
            return _inMemorySvc.Fetch(filter, excludeFilter) ?? Array.Empty<outPersonDTO>();
        }
    }
}
