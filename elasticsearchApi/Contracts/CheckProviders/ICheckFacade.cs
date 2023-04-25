using elasticsearchApi.Models.Person;

namespace elasticsearchApi.Contracts.CheckProviders
{
    public interface ICheckFacade
    {
        outPersonDTO? CallCheck(IDictionary<string, object?> filter);
    }
}
