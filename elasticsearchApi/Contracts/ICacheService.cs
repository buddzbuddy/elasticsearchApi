namespace elasticsearchApi.Contracts
{
    public interface ICacheService
    {
        IDictionary<int, int> GetRegCounters();
        void UpdateRegCounters(IDictionary<int, int> regCounters);
        //void ClearCache();

        object GetObject(string key);
        void UpdateObject(string key, object obj);
    }
}
