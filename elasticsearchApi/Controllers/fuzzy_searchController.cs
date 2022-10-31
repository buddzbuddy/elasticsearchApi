using Elasticsearch.Net;
using elasticsearchApi.Models;
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
                var settings = new ConnectionSettings(new Uri(_appSettings.Value.host)).DefaultIndex(_appSettings.Value.asist_persons_index_name);
                var client = new ElasticClient(settings);
                var filters = new List<Func<QueryContainerDescriptor<_asist_person>, QueryContainer>>();
                if (filter != null && filter.conditions != null && filter.conditions.Length > 0)
                {
                    foreach (var c in filter.conditions)
                    {
                        if (!string.IsNullOrEmpty(c.val))
                        {
                            if(typeof(_asist_person).GetProperty(c.field_name) != null)
                            {
                                if(c.field_name == "pin")
                                {
                                    filters.Add(fq => fq.Match(m =>
                                    m.Field(c.field_name)
                                    .Query(c.val)));
                                    continue;
                                }
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
                var searchDescriptor = new SearchDescriptor<_asist_person>()
                .From(0)
                .Size(10)
                .Query(q => q.Bool(b => b.Should(filters)));
                var json = client.RequestResponseSerializer.SerializeToString(searchDescriptor);
                WriteLog(json, true);

                var searchResponse = client.Search<_asist_person>(searchDescriptor);
                
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
                var settings = new ConnectionSettings(new Uri(_appSettings.Value.host)).DefaultIndex(_appSettings.Value.asist_persons_index_name);
                var client = new ElasticClient(settings);
                var filters = new List<Func<QueryContainerDescriptor<_asist_person>, QueryContainer>>();
                if (filter != null && filter.conditions != null && filter.conditions.Length > 0)
                {
                    foreach (var c in filter.conditions)
                    {
                        if (!string.IsNullOrEmpty(c.val))
                        {
                            if (typeof(_asist_person).GetProperty(c.field_name) != null)
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
                var searchDescriptor = new SearchDescriptor<_asist_person>()
                .From(0)
                .Size(10)
                .Query(q => q.Bool(b => b.Must(filters)));
                var json = client.RequestResponseSerializer.SerializeToString(searchDescriptor);
                WriteLog(json, true);

                var searchResponse = client.Search<_asist_person>(searchDescriptor);

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
        public ActionResult CreateAsistPerson([FromBody] Person obj)
        {
            var nrsz_connection_string = _appSettings.Value.cissa_data_connection;
            AttributeStorage attributeStorage = new AttributeStorage(nrsz_connection_string);
            attributeStorage.SavePerson(obj);
            //try
            //{

            //    var settings = new ConnectionSettings(new Uri(_appSettings.Value.host)).DefaultIndex(_appSettings.Value.asist_persons_index_name);
                
            //    var client = new ElasticClient(settings);

            //    client.CreateDocument(obj);

                return Ok(new { result = true });
            //}
            //catch (Exception e)
            //{
            //    return Ok(new { result = false, error = e.Message, trace = e.StackTrace });
            //}
        }

        [HttpPost]
        public ActionResult UpdateAsistPerson([FromBody] _asist_person obj)
        {
            try
            {
                var settings = new ConnectionSettings(new Uri(_appSettings.Value.host)).DefaultIndex(_appSettings.Value.asist_persons_index_name);
                var client = new ElasticClient(settings);

                var res = client.UpdateByQuery<_asist_person>(u => u
        .Query(q => q
            .Term(f => f.personid, obj.personid)
        )
        .Script("ctx._source.ln = '" + obj.ln + "'; ctx._source.fn = '" + obj.fn + "'; ctx._source.mn = '" + obj.mn + "'; ctx._source.pno = '" + obj.pno + "'; ctx._source.pin = '" + obj.pin + "';")
        .Conflicts(Conflicts.Proceed)
        .Refresh(true)
    );
                var json = client.RequestResponseSerializer.SerializeToString(res);
                WriteLog(json, true);
                return Ok(new { result = true });
            }
            catch (Exception e)
            {
                return Ok(new { result = false, error = e.Message, trace = e.StackTrace });
            }
        }


        [HttpPost]
        public ActionResult FindNrszPersons([FromBody] FilterModel filter)
        {
            try
            {
                var settings = new ConnectionSettings(new Uri(_appSettings.Value.host)).DefaultIndex(_appSettings.Value.nrsz_persons_index_name);
                var client = new ElasticClient(settings);
                var filters = new List<Func<QueryContainerDescriptor<_nrsz_person>, QueryContainer>>();
                if (filter != null && filter.conditions != null && filter.conditions.Length > 0)
                {
                    foreach (var c in filter.conditions)
                    {
                        if (!string.IsNullOrEmpty(c.val))
                        {
                            if (typeof(_nrsz_person).GetProperty(c.field_name) != null)
                            {
                                if (c.field_name == "pin")
                                {
                                    filters.Add(fq => fq.Match(m =>
                                    m.Field(c.field_name)
                                    .Query(c.val)));
                                    continue;
                                }
                                if(string.IsNullOrEmpty(c.operation))
                                filters.Add(fq => fq.Fuzzy(fz =>
                            fz.Field(c.field_name)
                            .Value(c.val)
                            .Fuzziness(Fuzziness.EditDistance(1))
                            .MaxExpansions(50)
                            .PrefixLength(0)
                            .Transpositions(true)
                            .Rewrite(MultiTermQueryRewrite.ConstantScore)
                            ));
                                else if (c.operation == "match")
                                {
                                    filters.Add(fq => fq.Match(m =>
                                    m.Field(c.field_name)
                                    .Query(c.val)));
                                }
                            }
                            else
                            {
                                throw new ApplicationException("Поле " + c.field_name + " не существует");
                            }
                        }
                    }
                }
                var searchDescriptor = new SearchDescriptor<_nrsz_person>()
                .From(0)
                .Size(10)
                .Query(q => q.Bool(b => b.Should(filters)));
                var json = client.RequestResponseSerializer.SerializeToString(searchDescriptor);
                WriteLog(json, true);

                var searchResponse = client.Search<_nrsz_person>(searchDescriptor);

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
        public ActionResult GetNrszPersons([FromBody] FilterModel filter)
        {
            try
            {
                var settings = new ConnectionSettings(new Uri(_appSettings.Value.host)).DefaultIndex(_appSettings.Value.nrsz_persons_index_name);
                var client = new ElasticClient(settings);
                var filters = new List<Func<QueryContainerDescriptor<_nrsz_person>, QueryContainer>>();
                if (filter != null && filter.conditions != null && filter.conditions.Length > 0)
                {
                    foreach (var c in filter.conditions)
                    {
                        if (!string.IsNullOrEmpty(c.val))
                        {
                            if (typeof(_nrsz_person).GetProperty(c.field_name) != null)
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
                var searchDescriptor = new SearchDescriptor<_nrsz_person>()
                .From(0)
                .Size(10)
                .Query(q => q.Bool(b => b.Must(filters)));
                var json = client.RequestResponseSerializer.SerializeToString(searchDescriptor);
                WriteLog(json, true);

                var searchResponse = client.Search<_nrsz_person>(searchDescriptor);

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
    public class _asist_person
    {
        public string personid { get; set; }
        public string pin { get; set; }
        public string ln { get; set; }
        public string fn { get; set; }
        public string mn { get; set; }
        public string pno { get; set; }
    }
    public class _nrsz_person
    {
        public string personid { get; set; }
        public string iin { get; set; }
        public string ln { get; set; }
        public string fn { get; set; }
        public string mn { get; set; }
        public string bd { get; set; }
        public string gender { get; set; }
        public string passporttype { get; set; }
        public string passportseries { get; set; }
        public string passportno { get; set; }
        public string date_of_issue { get; set; }
        public string issuing_authority { get; set; }
        public string familystate { get; set; }
    }
}
