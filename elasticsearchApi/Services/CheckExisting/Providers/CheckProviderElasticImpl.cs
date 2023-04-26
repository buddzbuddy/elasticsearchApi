using elasticsearchApi.Contracts;
using elasticsearchApi.Contracts.CheckProviders;
using elasticsearchApi.Models.Exceptions.CheckExisting;
using elasticsearchApi.Models.Person;
using elasticsearchApi.Utils;
using Nest;
using System;

namespace elasticsearchApi.Services.CheckExisting.Providers
{
    public class CheckProviderElasticImpl : ICheckProvider
    {
        private readonly IElasticService _es;
        public CheckProviderElasticImpl(IElasticService es)
        {
            _es = es;
        }
        public outPersonDTO[] FetchData(IDictionary<string, object> filter)
        {
            if (_es.FilterES(filter, out outPersonDTO[] es_data, out string[] errorMessages, out _))
            {
                return es_data;
            }
            else
            {
                throw new CheckExistingException("CheckElastic", $"Ошибка на стороне ElasticSearch Engine: {errorMessages.ToStringJoin()}");
            }
        }
    }
}
