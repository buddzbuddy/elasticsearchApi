using elasticsearchApi.Models.Person;

namespace elasticsearchApi.Contracts.CheckProviders
{
    public interface ICheckService
    {
        outPersonDTO? CheckExisting(ICheckProvider provider, IDictionary<string, object?> filter);
    }
}
