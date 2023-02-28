using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using elasticsearchApi.Utils;

namespace elasticsearchApi.Services
{
    public interface ICacheService
    {
        IDictionary<int, int> GetRegCounters();
        void UpdateRegCounters(IDictionary<int, int> regCounters);
        void ClearCache();

        object GetObject(string key);
        void UpdateObject(string key, object obj);
    }
    public class CacheService : ICacheService
    {
        private readonly ICacheProvider _cacheProvider;

        public CacheService(ICacheProvider cacheProvider)
        {
            _cacheProvider = cacheProvider;
        }

        public void ClearCache()
        {
            _cacheProvider.ClearCache(CacheKeys.RegCounters);
        }

        public IDictionary<int, int> GetRegCounters()
        {
            return _cacheProvider.GetFromCache<IDictionary<int, int>>(CacheKeys.RegCounters);
        }

        public void UpdateRegCounters(IDictionary<int, int> regCounters)
        {
            _cacheProvider.SetCache(CacheKeys.RegCounters, regCounters, DateTimeOffset.UtcNow.AddDays(1000));
        }

        public object GetObject(string key)
        {
            return _cacheProvider.GetFromCache<object>(key);
        }

        public void UpdateObject(string key, object obj)
        {
            _cacheProvider.SetCache(key, obj, DateTimeOffset.UtcNow.AddDays(100));
        }
    }
    public static class CacheKeys
    {
        public static string RegCounters => "_RegCounters";
    }
}
