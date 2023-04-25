namespace elasticsearchApi.Models.Contracts
{
    public interface IBaseEntity
    {
        object? this[string key] { get; set; }

        bool AreEquals(IDictionary<string, object?> filter);
    }
}
