using Elasticsearch.Net;
using elasticsearchApi.Utils;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Nest;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace elasticsearchApi.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class fuzzy_searchController : ControllerBase
    {
        private readonly IOptions<AppSettings> _appSettings;
        public fuzzy_searchController(IOptions<AppSettings> appSettings)
        {
            _appSettings = appSettings;
        }
        [HttpPost]
        public ActionResult FindAsistPersons([FromBody] FilterModel filter)
        {
            try
            {
                var settings = new ConnectionSettings(new Uri(_appSettings.Value.host)).DefaultIndex(_appSettings.Value.index_name);
                var client = new ElasticClient(settings);
                var filters = new List<Func<QueryContainerDescriptor<text_attribute>, QueryContainer>>();
                if (filter != null && filter.conditions != null && filter.conditions.Length > 0)
                {
                    foreach (var c in filter.conditions)
                    {
                        if (!string.IsNullOrEmpty(c.val))
                        {
                            if(typeof(text_attribute).GetProperty(c.field_name) != null)
                            {
                                filters.Add(fq => fq.Fuzzy(fz =>
                            fz.Field(c.field_name)
                            .Value(c.val)
                            .Fuzziness(Fuzziness.EditDistance(1))
                            .MaxExpansions(50)
                            .PrefixLength(0)
                            .Transpositions(true)
                            .Rewrite(MultiTermQueryRewrite.ConstantScore)
                            ));
                            }
                            else
                            {
                                throw new ApplicationException("Поле " + c.field_name + " не существует");
                            }
                        }
                    }
                }
                var searchDescriptor = new SearchDescriptor<text_attribute>()
                .From(0)
                .Size(10)
                .Query(q => q.Bool(b => b.Should(filters)));
                var json = client.RequestResponseSerializer.SerializeToString(searchDescriptor);
                WriteLog(json, true);

                var searchResponse = client.Search<text_attribute>(searchDescriptor);
                
                var persons = searchResponse.Documents;
                if (searchResponse.IsValid)
                    return Ok(new { result = true, persons });
                else
                    return Ok(new { result = false, error = searchResponse.OriginalException.Message, trace = searchResponse.OriginalException.StackTrace });
            }
            catch (Exception e)
            {
                return Ok(new { result = false, error = e.Message, trace = e.StackTrace });
            }
        }
        [HttpPost]
        public ActionResult GetAsistPersons([FromBody] FilterModel filter)
        {
            try
            {
                var settings = new ConnectionSettings(new Uri(_appSettings.Value.host)).DefaultIndex(_appSettings.Value.index_name);
                var client = new ElasticClient(settings);
                var filters = new List<Func<QueryContainerDescriptor<text_attribute>, QueryContainer>>();
                if (filter != null && filter.conditions != null && filter.conditions.Length > 0)
                {
                    foreach (var c in filter.conditions)
                    {
                        if (!string.IsNullOrEmpty(c.val))
                        {
                            if (typeof(text_attribute).GetProperty(c.field_name) != null)
                            {
                                filters.Add(fq => fq.Match(m =>
                                    m.Field(c.field_name)
                                    .Query(c.val)
                            ));
                            }
                            else
                            {
                                throw new ApplicationException("Поле " + c.field_name + " не существует");
                            }
                        }
                    }
                }
                var searchDescriptor = new SearchDescriptor<text_attribute>()
                .From(0)
                .Size(10)
                .Query(q => q.Bool(b => b.Must(filters)));
                var json = client.RequestResponseSerializer.SerializeToString(searchDescriptor);
                WriteLog(json, true);

                var searchResponse = client.Search<text_attribute>(searchDescriptor);

                var persons = searchResponse.Documents;
                if (searchResponse.IsValid)
                    return Ok(new { result = true, persons });
                else
                    return Ok(new { result = false, error = searchResponse.OriginalException.Message });
            }
            catch (Exception e)
            {
                return Ok(new { result = false, error = e.Message, trace = e.StackTrace });
            }
        }

        [HttpPost]
        public ActionResult CreateDocument([FromBody] text_attribute obj)
        {
            try
            {
                var settings = new ConnectionSettings(new Uri(_appSettings.Value.host)).DefaultIndex(_appSettings.Value.index_name);
                var client = new ElasticClient(settings);

                client.CreateDocument(obj);

                return Ok(new { result = true });
            }
            catch (Exception e)
            {
                return Ok(new { result = false, error = e.Message, trace = e.StackTrace });
            }
        }

        [HttpPost]
        public ActionResult UpdateDocument([FromBody] text_attribute obj)
        {
            try
            {
                var settings = new ConnectionSettings(new Uri(_appSettings.Value.host)).DefaultIndex(_appSettings.Value.index_name);
                var client = new ElasticClient(settings);

                client.UpdateByQuery<text_attribute>(u => u
        .Query(q => q
            .Term(f => f.personid, obj.personid)
        )
        .Script("ctx._source.ln = '" + obj.ln + "'; ctx._source.fn = '" + obj.fn + "'; ctx._source.mn = '" + obj.mn + "'; ctx._source.pNo = '" + obj.pno + "';")
        .Conflicts(Conflicts.Proceed)
        .Refresh(true)
    );

                return Ok(new { result = true });
            }
            catch (Exception e)
            {
                return Ok(new { result = false, error = e.Message, trace = e.StackTrace });
            }
        }
        public void WriteLog(string src, bool withLog = false)
        {
            if(withLog)
            using (StreamWriter sw = new StreamWriter(_appSettings.Value.logpath, true))
            {
                sw.WriteLine(DateTime.Now.ToString() + "-ElasticSearchAPI:\n" + src);
            }
        }
        public class FilterModel
        {
            public _condition[] conditions { get; set; }
            public class _condition
            {
                public string field_name { get; set; }
                public string operation { get; set; }
                public string val { get; set; }
            }
        }
    }
    public class text_attribute
    {
        public string personid { get; set; }
        public string ln { get; set; }
        public string fn { get; set; }
        public string mn { get; set; }
        public string pno { get; set; }
    }
}
