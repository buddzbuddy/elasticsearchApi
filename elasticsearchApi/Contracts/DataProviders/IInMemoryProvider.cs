using elasticsearchApi.Models.Person;

namespace elasticsearchApi.Contracts.DataProviders
{
    public interface IInMemoryProvider
    {
        void Save(outPersonDTO personDTO);
        void Save(outPersonDTO personDTO, DateTimeOffset expirationAbsoluteTime);
        outPersonDTO[]? Fetch(IDictionary<string, object?> filter);
    }
}
