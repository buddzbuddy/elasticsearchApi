using elasticsearchApi.Contracts;
using elasticsearchApi.Contracts.CheckProviders;
using elasticsearchApi.Contracts.DataProviders;
using elasticsearchApi.Models.Person;

namespace elasticsearchApi.Services.CheckExisting.Providers
{
    public class CheckMemory : ICheckProvider
    {
        private readonly IInMemoryProvider _inMemorySvc;
        public CheckMemory(IInMemoryProvider inMemorySvc)
        {
            _inMemorySvc = inMemorySvc;
        }
        public outPersonDTO[] FetchData(IDictionary<string, object?> filter)
        {
            return _inMemorySvc.Fetch(filter) ?? Array.Empty<outPersonDTO>();
        }
    }
}
