using Elasticsearch.Net;
using elasticsearchApi.Models;
using elasticsearchApi.Utils;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Nest;
using System;
using System.Collections.Generic;
using System.ComponentModel;
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
            DbStorage attributeStorage = new DbStorage(nrsz_connection_string);
            attributeStorage.InsertPerson(obj);
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
        public ActionResult UpdateAsistPerson2([FromBody] Person obj)
        {
            var nrsz_connection_string = _appSettings.Value.cissa_data_connection;
            DbStorage attributeStorage = new DbStorage(nrsz_connection_string);
            //attributeStorage.UpdatePerson(obj);
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
        public ActionResult UpdateNrszPerson([FromBody] personDTO obj)
        {
            try
            {
                var settings = new ConnectionSettings(new Uri(_appSettings.Value.host)).DefaultIndex(_appSettings.Value.asist_persons_index_name);
                var client = new ElasticClient(settings);

                var res = client.UpdateByQuery<personDTO>(u => u
        .Query(q => q
            .Term(f => f.id, obj.id)
        )
        .Script("ctx._source.ln = '" + obj.last_name + "'; ctx._source.fn = '" + obj.first_name + "'; ctx._source.mn = '" + obj.middle_name + "'; ctx._source.passportno = '" + obj.passportno + "'; ctx._source.iin = '" + obj.iin + "';")
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
                var filters = new List<Func<QueryContainerDescriptor<personDTO>, QueryContainer>>();
                if (filter != null && filter.conditions != null && filter.conditions.Length > 0)
                {
                    foreach (var c in filter.conditions)
                    {
                        if (!string.IsNullOrEmpty(c.val))
                        {
                            if (typeof(personDTO).GetProperty(c.field_name) != null)
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
                var searchDescriptor = new SearchDescriptor<personDTO>()
                .From(0)
                .Size(10)
                .Query(q => q.Bool(b => b.Should(filters)));
                var json = client.RequestResponseSerializer.SerializeToString(searchDescriptor);
                WriteLog(json, true);

                var searchResponse = client.Search<personDTO>(searchDescriptor);

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
                var filters = new List<Func<QueryContainerDescriptor<personDTO>, QueryContainer>>();
                if (filter != null && filter.conditions != null && filter.conditions.Length > 0)
                {
                    foreach (var c in filter.conditions)
                    {
                        if (!string.IsNullOrEmpty(c.val))
                        {
                            if (typeof(personDTO).GetProperty(c.field_name) != null)
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
                var searchDescriptor = new SearchDescriptor<personDTO>()
                .From(0)
                .Size(10)
                .Query(q => q.Bool(b => b.Must(filters)));
                var json = client.RequestResponseSerializer.SerializeToString(searchDescriptor);
                WriteLog(json, true);

                var searchResponse = client.Search<personDTO>(searchDescriptor);

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
        private void WriteLog(string src, bool withLog = false)
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
    public class personDTO
    {
        public Guid id { get; set; }
        [Description("IIN")]
        public string iin { get; set; }
        [Description("Last_Name")]
        public string last_name { get; set; }
        [Description("First_Name")]
        public string first_name { get; set; }
        [Description("Middle_Name")]
        public string middle_name { get; set; }
        [Description("Date_of_Birth")]
        public DateTime? date_of_birth { get; set; }
        [Description("Sex")]
        public Guid? sex { get; set; }
        [Description("PassportType")]
        public Guid? passporttype { get; set; }
        [Description("PassportSeries")]
        public string passportseries { get; set; }
        [Description("PassportNo")]
        public string passportno { get; set; }
        [Description("Date_of_Issue")]
        public DateTime? date_of_issue { get; set; }
        [Description("Issuing_Authority")]
        public string issuing_authority { get; set; }
        [Description("FamilyState")]
        public Guid? familystate { get; set; }
    }
}
