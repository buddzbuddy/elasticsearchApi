using elasticsearchApi.Models.Filters;

namespace elasticsearchApi.Models.Contracts
{
    public interface IBaseEntity
    {
        object? this[string key] { get; set; }

        bool Equals(IDictionary<string, object?> filter);
    }
}
