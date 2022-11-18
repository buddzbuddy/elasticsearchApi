using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Data.SqlClient;
using elasticsearchApi.Models;
using SqlKata.Compilers;
using SqlKata.Execution;
using System.Data;
using Nest;
using elasticsearchApi.Controllers;
using Elasticsearch.Net;
using SqlKata;
using AutoMapper;

namespace elasticsearchApi.Utils
{
    public class AsistService
    {
        private readonly AppSettings _appSettings;
        public AsistService(AppSettings appSettings) => _appSettings = appSettings;

        public class ServiceContext
        {
            public bool SuccessFlag { get; set; }

            public IDictionary<string, string> ErrorMessages { get; set; } = new Dictionary<string, string>();

            internal void AddErrorMessage(string key, string errorMessage)
            {
                if (!ErrorMessages.ContainsKey(key))
                    ErrorMessages.Add(key, errorMessage);
                else
                    ErrorMessages[key] += "; " + errorMessage;
            }

            public IDictionary<string, object> Data { get; set; } = new Dictionary<string, object>();

            internal object this[string attributeName]
            {
                get
                {
                    return Data.ContainsKey(attributeName) ? Data[attributeName] : null;
                }
                set
                {
                    if (Data.ContainsKey(attributeName)) Data[attributeName] = value;
                    else Data.Add(attributeName, value);
                }
            }
        }

        public _nrsz_person[] SomePersons()
        {
            var settings = new ConnectionSettings(new Uri(_appSettings.host)).DefaultIndex(_appSettings.asist_persons_index_name);
            var client = new ElasticClient(settings);
            
            var searchResponse = client.Search<_nrsz_person>(new SearchRequest { Size = 10 });
            var persons = searchResponse.Documents;
            return persons.ToArray();
        }
        public ServiceContext FindSamePersonES(SearchPersonModel person)
        {
            //WorkflowContext context = CreateContext("asist2nrsz", new Guid("{05EEF54F-5BFE-4E2B-82C7-6AB6CD59D488}"));
            ServiceContext context = new();
            
            
            verifyData(context, person);
            context.SuccessFlag = context.ErrorMessages == null || context.ErrorMessages.Count == 0;
            if (context.SuccessFlag)
            {
                var settings = new ConnectionSettings(new Uri(_appSettings.host)).DefaultIndex(_appSettings.asist_persons_index_name);
                var client = new ElasticClient(settings);
                var filters = new List<Func<QueryContainerDescriptor<_nrsz_person>, QueryContainer>>();
                foreach (var propInfo in person.GetType().GetProperties(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.IgnoreCase))
                {
                    var field_name = propInfo.Name.ToLower();
                    if (field_name != "id" && propInfo.GetValue(person) != null && !string.IsNullOrEmpty(propInfo.GetValue(person).ToString()))
                        filters.Add(fq => fq.Match(m => m.Field(field_name).Query(propInfo.GetValue(person).ToString())));
                }
                if (filters.Count == 0)
                    throw new ApplicationException("Данные для поиска не переданы!");
                var searchDescriptor = new SearchDescriptor<_nrsz_person>()
                .From(0)
                .Size(10)
                .Query(q => q.Bool(b => b.Must(filters)));
                var json = client.RequestResponseSerializer.SerializeToString(searchDescriptor);
                WriteLog(json, _appSettings.logpath);

                var searchResponse = client.Search<_nrsz_person>(searchDescriptor);

                var persons = searchResponse.Documents;
                if (searchResponse.IsValid)
                {
                    var dupPersonList = persons;
                    if (dupPersonList.Count == 1)
                    {
                        context["Exactly"] = true;
                        context["ResultCount"] = 1;
                        context["Result"] = dupPersonList.First();
                    }
                    else
                    {
                        context["Exactly"] = false;
                        context["ResultCount"] = dupPersonList.Count;
                    }
                }
            }
            return context;
        }

        private static void verifyData(ServiceContext context, SearchPersonModel person)
        {
            System.Text.RegularExpressions.Regex nameRegex =
                new System.Text.RegularExpressions.Regex("[0-9]");
            object s = person.last_name;
            string lastName = (s != null ? s.ToString() : String.Empty).Trim().ToLower();

            if (String.IsNullOrEmpty(lastName)) //message = "Заполните фамилию!\n";
                context.AddErrorMessage("Last_Name", "Заполните фамилию");
            else
            {
                lastName = char.ToUpper(lastName[0]) + lastName.Substring(1);
                person.last_name = lastName;
                if (nameRegex.IsMatch(lastName))
                    context.AddErrorMessage("Last_Name", "Ошибка в формате фамилии! Должны быть только буквы.");
            }

            s = person.first_name;
            var firstName = (s != null ? s.ToString() : String.Empty).Trim().ToLower();

            if (String.IsNullOrEmpty(firstName))
                context.AddErrorMessage("First_Name", "Заполните имя");
            else
            {
                firstName = char.ToUpper(firstName[0]) + firstName.Substring(1);
                person.first_name = firstName;
                if (nameRegex.IsMatch(firstName))
                    context.AddErrorMessage("First_Name", "Ошибка в формате имени! Должны быть только буквы.");
            }

            s = person.middle_name;
            var middleName = (s != null ? s.ToString() : String.Empty).Trim().ToLower();

            if (!String.IsNullOrEmpty(middleName))
            {
                middleName = char.ToUpper(middleName[0]) + middleName.Substring(1);
                person.middle_name = middleName;
                if (nameRegex.IsMatch(middleName))
                    context.AddErrorMessage("Middle_Name", "Ошибка в формате отчества! Должны быть только буквы.");
            }

            System.Text.RegularExpressions.Regex regex =
                new System.Text.RegularExpressions.Regex("[^0-9]");
            s = person.iin;
            var pin = (s != null ? s.ToString() : String.Empty).Trim().ToLower();

            if (!String.IsNullOrEmpty(pin))
            {
                if (pin.Length > 14)
                    context.AddErrorMessage("IIN", "Ошибка в длине ПИН");
                if (regex.IsMatch(pin))
                    context.AddErrorMessage("IIN", "Ошибка в формате номера ПИН! Должны быть только цифры");
                context["PIN"] = pin;
            }

            /*s = person.SIN;
            var sin = (s != null ? s.ToString() : String.Empty).Trim().ToLower();

            if (!String.IsNullOrEmpty(sin))
            {
                if (regex.IsMatch(sin.Substring(1)))
                    context.AddErrorMessage("SIN", "Ошибка в формате номера СИН!");
                context["SIN"] = sin;
            }*/

            s = person.sex;
            if (s == null) //message += "Введите пол!\n"; 
                context.AddErrorMessage("Sex", "Введите пол");

            s = person.date_of_birth;
            if (s == null) //message += "Введите дату рождения!\n"; 
                context.AddErrorMessage("Date_of_Birth", "Введите дату рождения");

        }

        public ServiceContext FindPersonsES(SearchPersonModel person)
        {
            ServiceContext context = new();
            context.SuccessFlag = false;
            //context.SuccessFlag = context.ErrorMessages == null || context.ErrorMessages.Count == 0;
            //verifyData(context, person);
            //if (context.SuccessFlag)
            {
                var settings = new ConnectionSettings(new Uri(_appSettings.host)).DefaultIndex(_appSettings.asist_persons_index_name);
                var client = new ElasticClient(settings);
                var filters = new List<Func<QueryContainerDescriptor<_nrsz_person>, QueryContainer>>();
                foreach (var propInfo in person.GetType().GetProperties(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.IgnoreCase))
                {
                    var field_name = propInfo.Name.ToLower();
                    if (field_name != "id" && propInfo.GetValue(person) != null && !string.IsNullOrEmpty(propInfo.GetValue(person).ToString()))
                        filters.Add(fq => fq.Match(m => m.Field(field_name).Query(propInfo.GetValue(person).ToString())));
                }
                if (filters.Count == 0)
                {
                    //throw new ApplicationException("Данные для поиска не переданы!");
                    context.AddErrorMessage("", "Данные для поиска не переданы!");
                    context.SuccessFlag = false;
                    context["Persons"] = new List<object>();
                    return context;
                }
                var searchDescriptor = new SearchDescriptor<_nrsz_person>()
                .From(0)
                .Size(10)
                .Query(q => q.Bool(b => b.Must(filters)));
                var json = client.RequestResponseSerializer.SerializeToString(searchDescriptor);
                WriteLog(json, _appSettings.logpath);

                var searchResponse = client.Search<_nrsz_person>(searchDescriptor);

                var persons = searchResponse.Documents;
                if (searchResponse.IsValid)
                {
                    context["Persons"] = persons;
                    context.SuccessFlag = true;
                }
                else
                {
                    context.AddErrorMessage("", searchResponse.OriginalException.Message);
                }
            }
            return context;
        }

        public bool FilterES(IDictionary<string, string> filter, out _nrsz_person[] data, out string[] errorMessages)
        {
            errorMessages = Array.Empty<string>();
            data = null;//Array.Empty<_nrsz_person>();
            var settings = new ConnectionSettings(new Uri(_appSettings.host)).DefaultIndex(_appSettings.asist_persons_index_name);
            var client = new ElasticClient(settings);
            var filters = new List<Func<QueryContainerDescriptor<_nrsz_person>, QueryContainer>>();
            foreach (var f in filter)
            {
                if (Guid.TryParse(f.Value, out Guid g) && g != Guid.Empty)
                {
                    filters.Add(fq => fq.MatchPhrase(tq =>
                        tq.Field(f.Key).Query(f.Value)));
                }
                else
                    filters.Add(fq => fq.Match(m => m.Field(f.Key).Query(f.Value)));
            }

            if (filters.Count == 0)
            {
                errorMessages[0] = "Пустой поиск запрещен!";
                return false;
            }
            var searchDescriptor = new SearchDescriptor<_nrsz_person>()
            /*.From(0)
            .Size(10)*/
            .Query(q => q.Bool(b => b.Must(filters)));
            var json = client.RequestResponseSerializer.SerializeToString(searchDescriptor);
            WriteLog(json, _appSettings.logpath);

            var searchResponse = client.Search<_nrsz_person>(searchDescriptor);

            var persons = searchResponse.Documents;
            if (searchResponse.IsValid)
            {
                data = persons.ToArray();
                return true;
            }
            else
            {
                errorMessages[0] = searchResponse.OriginalException.Message;
                return false;
            }
        }

        public bool Fuzzy(IDictionary<string, string> filter, out _nrsz_person[] data, out string[] errorMessages)
        {
            errorMessages = Array.Empty<string>();
            data = null;//Array.Empty<_nrsz_person>();
            var settings = new ConnectionSettings(new Uri(_appSettings.host)).DefaultIndex(_appSettings.asist_persons_index_name);
            var client = new ElasticClient(settings);
            var filters = new List<Func<QueryContainerDescriptor<_nrsz_person>, QueryContainer>>();
            foreach (var f in filter)
            {
                filters.Add(fq => fq.Fuzzy(fz =>
                            fz.Field(f.Key)
                            .Value(f.Value)
                            .Fuzziness(Fuzziness.Auto)
                            .MaxExpansions(50)
                            .PrefixLength(0)
                            .Transpositions(true)
                            .Rewrite(MultiTermQueryRewrite.ConstantScore)
                            ));
            }

            if (filters.Count == 0)
            {
                errorMessages[0] = "Пустой поиск запрещен!";
                return false;
            }
            var searchDescriptor = new SearchDescriptor<_nrsz_person>()
            /*.From(0)
            .Size(10)*/
            .Query(q => q.Bool(b => b.Must(filters)));
            var json = client.RequestResponseSerializer.SerializeToString(searchDescriptor);
            WriteLog(json, _appSettings.logpath);

            var searchResponse = client.Search<_nrsz_person>(searchDescriptor);

            var persons = searchResponse.Documents;
            if (searchResponse.IsValid)
            {
                data = persons.ToArray();
                return true;
            }
            else
            {
                errorMessages[0] = searchResponse.OriginalException.Message;
                return false;
            }
        }

        public ServiceContext FindPersonByPINES(SearchPersonModel person)
        {
            ServiceContext context = new()
            {
                SuccessFlag = false
            };

            System.Text.RegularExpressions.Regex regex =
                new("[^0-9]");

            if (!string.IsNullOrEmpty(person.iin))
            {
                var settings = new ConnectionSettings(new Uri(_appSettings.host)).DefaultIndex(_appSettings.asist_persons_index_name);
                var client = new ElasticClient(settings);
                var filters = new List<Func<QueryContainerDescriptor<_nrsz_person>, QueryContainer>>
                {
                    fq => fq.Match(m => m.Field("iin").Query(person.iin))
                };
                if (filters.Count == 0)
                    throw new ApplicationException("Данные для поиска не переданы!");
                var searchDescriptor = new SearchDescriptor<_nrsz_person>()
                .From(0)
                .Size(1)
                .Query(q => q.Bool(b => b.Must(filters)));
                var json = client.RequestResponseSerializer.SerializeToString(searchDescriptor);
                WriteLog(json, _appSettings.logpath);

                var searchResponse = client.Search<_nrsz_person>(searchDescriptor);

                var persons = searchResponse.Documents;
                if (searchResponse.IsValid)
                {
                    var res = persons.FirstOrDefault();
                    context["Result"] = res;
                    context.SuccessFlag = res != null;
                }
            }
            return context;
        }
        static object lockObj = new object();
        public static void WriteLog(string text, string pathToFile = "c:\\distr\\cissa\\new-nrsz-rest-api.log")
        {
            lock (lockObj)
            {
                using (StreamWriter sw = new StreamWriter(pathToFile, true))
                {
                    sw.WriteLine(text);
                }
            }
        }
    }

}
