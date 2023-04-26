using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using elasticsearchApi.Contracts;
using elasticsearchApi.Utils;

namespace elasticsearchApi.Services
{
    public class CacheServiceImpl : ICacheService
    {
        private readonly ICacheProvider _cacheProvider;

        private const int MAX_DURATION_DAYS = 100;
        public CacheServiceImpl(ICacheProvider cacheProvider)
        {
            _cacheProvider = cacheProvider;
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
            UpdateObject(key, obj, DateTimeOffset.UtcNow.AddDays(MAX_DURATION_DAYS));
        }

        public void UpdateObject(string key, object obj, DateTimeOffset expirationAbsoluteTime)
        {
            _cacheProvider.SetCache(key, obj, expirationAbsoluteTime);
        }
    }
    public class CacheKeys
    {
        public const string RegCounters = "_RegCounters";
        public const string ADDRESS_REFS_KEY = "_ADDRESS_REFS_KEY";
        public const string TEMP_PERSONS = "_TEMP_PERSONS";
    }
}
