using elasticsearchApi.Contracts.CheckProviders;
using elasticsearchApi.Models.Infrastructure;
using elasticsearchApi.Models.Person;
using SqlKata.Execution;

namespace elasticsearchApi.Services.CheckExisting.Providers
{
    public class CheckProviderDatabaseImpl : ICheckProvider
    {
        private readonly QueryFactory _queryFactory;
        private readonly AppTransaction _appTransaction;
        public CheckProviderDatabaseImpl(QueryFactory queryFactory, AppTransaction appTransaction)
        {
            _queryFactory = queryFactory;
            _appTransaction = appTransaction;
        }
        public outPersonDTO[] FetchData(IDictionary<string, object?> filter, IDictionary<string, object?>? excludeFilter = null)
        {
            if((filter.Count > 0) || (excludeFilter != null && excludeFilter.Count > 0))
            {
                var result = Array.Empty<outPersonDTO>();

                var query = _queryFactory.Query("Persons");
                if(filter.Count > 0)
                {
                    query = query.Where(filter);
                }
                if(excludeFilter != null && excludeFilter.Count > 0)
                {
                    query= query.Where(excludeFilter);
                }

                result = query.Get<outPersonDTO>(_appTransaction.Transaction).ToArray();

                return result;
            }
            else 
                throw new ArgumentNullException($"Для поиска данных в базе данных не переданы параметры фильтра");
        }
    }
}
