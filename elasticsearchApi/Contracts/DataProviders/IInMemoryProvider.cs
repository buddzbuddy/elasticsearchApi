using elasticsearchApi.Models.Person;

namespace elasticsearchApi.Contracts.DataProviders
{
    public interface IInMemoryProvider
    {
        void Save(outPersonDTO personDTO, int? lifetimeInSeconds = null);
        outPersonDTO[]? Fetch(IDictionary<string, object?> filter, IDictionary<string, object?>? excludeFilter = null);
    }
}
