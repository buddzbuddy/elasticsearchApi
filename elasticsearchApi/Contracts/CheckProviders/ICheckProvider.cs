using elasticsearchApi.Models.Person;

namespace elasticsearchApi.Contracts.CheckProviders
{
    public interface ICheckProvider
    {
        outPersonDTO[] FetchData(IDictionary<string, object?> filter);
    }
}
