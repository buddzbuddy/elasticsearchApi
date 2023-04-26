using elasticsearchApi.Contracts.CheckProviders;
using elasticsearchApi.Models.Person;

namespace elasticsearchApi.Services.CheckExisting
{
    public class CheckServiceImpl : ICheckService
    {
        public outPersonDTO? CheckExisting(ICheckProvider provider, IDictionary<string, object?> filter)
        {
            var data = provider.FetchData(filter);
            if(data != null && data.Length > 0)
            {
                return data[0];
            }
            return null;
        }
    }
}
