using elasticsearchApi.Contracts;
using elasticsearchApi.Contracts.DataProviders;
using elasticsearchApi.Models.Person;

namespace elasticsearchApi.Services.DataProviders
{
    public class InMemoryProviderImpl : IInMemoryProvider
    {
        private readonly ICacheService _cacheSvc;
        public InMemoryProviderImpl(ICacheService cacheSvc)
        {
            _cacheSvc = cacheSvc;
        }

        public const int MAX_DURATION_IN_MINUTES = 6;
        public const int MAX_DURATION_IN_SECONDS = MAX_DURATION_IN_MINUTES * 60;
        public outPersonDTO[]? Fetch(IDictionary<string, object?> filter)
        {
            var allPersons = _cacheSvc.GetObject(CacheKeys.TEMP_PERSONS) as HashSet<outPersonDTO> ?? new HashSet<outPersonDTO>();

            if(filter == null || filter.Count == 0)
            {
                return allPersons.ToArray();
            }

            var filteredPersons = new List<outPersonDTO>();
            if(allPersons != null)
            {
                foreach (var p in allPersons)
                {
                    if(p.Equals(filter)) filteredPersons.Add(p);
                }
            }

            return filteredPersons.ToArray();
        }

        public void Save(outPersonDTO personDTO, int? lifetimeInSeconds = null) => _save(personDTO, DateTimeOffset.UtcNow.AddSeconds(lifetimeInSeconds ?? MAX_DURATION_IN_SECONDS));

        private void _save(outPersonDTO personDTO, DateTimeOffset expirationAbsoluteTime)
        {
            var allPersons = _cacheSvc.GetObject(CacheKeys.TEMP_PERSONS) as HashSet<outPersonDTO> ?? new HashSet<outPersonDTO>();
            if (allPersons.Add(personDTO))
                _cacheSvc.UpdateObject(CacheKeys.TEMP_PERSONS, allPersons, expirationAbsoluteTime);
        }
    }
}
