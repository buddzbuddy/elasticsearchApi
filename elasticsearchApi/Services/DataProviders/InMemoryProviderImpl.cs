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
        public outPersonDTO[]? Fetch(IDictionary<string, object?> filter)
        {
            var allPersons = _cacheSvc.GetObject(CacheKeys.TEMP_PERSONS) as HashSet<outPersonDTO> ?? new HashSet<outPersonDTO>();

            if(filter == null)
            {
                return allPersons.ToArray();
            }

            var filteredPersons = new List<outPersonDTO>();
            if(allPersons != null)
            {
                foreach (var p in allPersons)
                {
                    if(p.AreEquals(filter)) filteredPersons.Add(p);
                }
            }

            return filteredPersons.ToArray();
        }

        public void Save(outPersonDTO personDTO)
        {
            var allPersons = _cacheSvc.GetObject(CacheKeys.TEMP_PERSONS) as HashSet<outPersonDTO> ?? new HashSet<outPersonDTO>();
            if (allPersons.Add(personDTO))
                _cacheSvc.UpdateObject(CacheKeys.TEMP_PERSONS, allPersons);
        }
    }
}
