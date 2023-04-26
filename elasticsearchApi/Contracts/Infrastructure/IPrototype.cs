namespace elasticsearchApi.Contracts.Infrastructure
{
    public interface IPrototype<T>
    {
        T CreateDeepCopy();
    }
}
