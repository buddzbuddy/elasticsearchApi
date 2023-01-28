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
using System.ComponentModel;
using System.Text.Json;
using System.Threading;
using Microsoft.AspNetCore.Components.Forms;
using static System.Net.WebRequestMethods;

namespace elasticsearchApi.Utils
{
    public class ElasticService
    {
        private readonly string host;
        private readonly string index;
        private readonly bool logEnabled;
        private readonly string logPath;
        private readonly IMapper _mapper;
        private readonly AppSettings _appSettings;
        private readonly ElasticClient _client;
        public ElasticService(string _host, string _index, bool _logEnabled, string _logPath, IMapper mapper, AppSettings appSettings, ElasticClient client)
        {
            host = _host;
            index = _index;
            logEnabled = _logEnabled;
            logPath = _logPath;
            _mapper = mapper;
            _appSettings = appSettings;
            _client = client;
        }

        

        static IDictionary<string, object> ModelToDict<T>(T obj)
        {
            Dictionary<string, object> dict = new();
            foreach (var propInfo in typeof(T).GetProperties(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.IgnoreCase))
            {
                var field_name = propInfo.Name.ToLower();
                if (/*field_name != "id" && */propInfo.GetValue(obj) != null && !string.IsNullOrEmpty(propInfo.GetValue(obj).ToString()))
                    dict.Add(field_name, propInfo.GetValue(obj));
            }
            return dict;
        }
        static documentDTO ModelToDocumentWithAttributes<T>(T obj)
        {
            var res = new documentDTO
            {
                attributes = Array.Empty<attributeDTO>()
            };
            foreach (var propInfo in typeof(T).GetProperties(System.Reflection.BindingFlags.IgnoreCase | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public))
            {
                var v = propInfo.GetValue(obj);
                if (propInfo.Name == "id" || v.IsNullOrEmpty()) continue;
                if (propInfo.Name == "id" && v != null && v is int id && id > 0)
                {
                    res.id = id;
                }
                else
                {
                    var attr = new attributeDTO { name = (propInfo.GetCustomAttributes(true)[0] as DescriptionAttribute).Description, value = v };
                    res.attributes = res.attributes.Append(attr);
                }
            }
            return res;
        }
        static bool DocumentToDict(documentDTO obj, out IDictionary<string, object> dict, out string[] errorMessages)
        {
            errorMessages = Array.Empty<string>();
            dict = new Dictionary<string, object>();
            if (obj != null)
            {
                if (obj.id != null) dict["id"] = obj.id;
                foreach (var attr in obj.attributes)
                {
                    if (attr.value.IsNullOrEmpty()) continue;
                    var attrname = attr.name.ToLower();
                    if (!dict.ContainsKey(attrname))
                    {
                        dict[attrname] = attr.value.ToString();
                    }
                    else
                    {
                        errorMessages = errorMessages.Append($"Поле {attr.name} дублируется в теле запроса! Дубликаты полей недопустимы!").ToArray();
                    }
                }
            }
            return errorMessages.Length == 0;
        }
        public void FindSamePersonES(SearchPersonModel inputData, ref ServiceContext context, int page = 1, int size = 10)
        {
            verifyData(ref context, inputData);
            context.SuccessFlag = context.ErrorMessages == null || context.ErrorMessages.Count == 0;
            if (context.SuccessFlag)
            {
                
                if (FilterES(ModelToDict(inputData), out personDTO[] data, out string[] errorMessages, false, page, size))
                {
                    var dupPersonList = data;
                    if (dupPersonList.Count() == 1)
                    {
                        context["Exactly"] = true;
                        context["ResultCount"] = 1;
                        context["Result"] = dupPersonList.First();
                    }
                    else
                    {
                        context["Exactly"] = false;
                        context["ResultCount"] = dupPersonList.Count();
                    }
                }
                else
                {
                    throw new ApplicationException(string.Join(", ", errorMessages));
                }
            }
        }

        public void FindPersonsES(SearchPersonModel inputData, ref ServiceContext context, bool fuzzy = false, int page = 1, int size = 10)
        {
            verifyData(ref context, inputData);
            context.SuccessFlag = context.ErrorMessages == null || context.ErrorMessages.Count == 0;
            if (context.SuccessFlag)
            {

                if (FilterES(ModelToDict(inputData), out personDTO[] data, out string[] errorMessages, fuzzy, page, size))
                {
                    context["Persons"] = data;
                    context.SuccessFlag = true;
                }
                else
                {
                    throw new ApplicationException(string.Join(", ", errorMessages));
                }
            }
        }

        private static void verifyData(ref ServiceContext context, SearchPersonModel person)
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


        public bool FilterES(IDictionary<string, object> filter, out personDTO[] data, out string[] errorMessages, bool fuzzy = false, int page = 1, int size = 10)
        {
            errorMessages = Array.Empty<string>();
            data = Array.Empty<personDTO>();
            var filters = new List<Func<QueryContainerDescriptor<personDTO>, QueryContainer>>();
            foreach (var f in filter)
            {
                var val = f.Value;
                if (val == null || string.IsNullOrEmpty(val.ToString())) continue;

                //predefined excpectations
                //DateTime Cases
                if (f.Key.ToLower().Contains("date") || f.Key.EndsWith("At") || f.Key.ToLower().Contains("time"))
                {
                    val = val.ToString().Split('T')[0];
                }
                //Convert input date to UTC date like in ES
                if (DateTime.TryParseExact(val.ToString(), "yyyy-MM-dd", System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None, out DateTime d))
                {
                    //Console.WriteLine(d.ToString());
                    //var utcDate = d.ToUniversalTime().ToString("yyyy-MM-ddTHH:mm:ssZ");
                    //Console.WriteLine(utcDate);
                    filters.Add(fq => fq.Match(m => m.Field(f.Key).Query(d.ToString("yyyy-MM-dd"))));
                }
                else if (Guid.TryParse(val.ToString(), out Guid g) && g != Guid.Empty)
                {
                    filters.Add(fq => fq.MatchPhrase(tq => tq.Field(f.Key).Query(val.ToString())));
                }
                else
                {
                    if (fuzzy)
                        filters.Add(fq => fq.Fuzzy(fz =>
                                fz.Field(f.Key)
                                .Value(val.ToString())
                                .Fuzziness(Fuzziness.Auto)
                                .MaxExpansions(50)
                                .PrefixLength(0)
                                .Transpositions(true)
                                .Rewrite(MultiTermQueryRewrite.ConstantScore)
                                ));
                    else
                        filters.Add(fq => fq.Match(m => m.Field(f.Key).Query(val.ToString())));
                }
            }

            if (filters.Count == 0)
            {
                errorMessages = errorMessages.Append("Пустой поиск запрещен!").ToArray();
                return false;
            }
            var searchDescriptor = new SearchDescriptor<personDTO>()
            .From(page - 1)
            .Size(size)
            .Query(q => q.Bool(b => b.Must(filters)));
            
            if (logEnabled)
            {
                var json = _client.RequestResponseSerializer.SerializeToString(searchDescriptor);
                WriteLog($"[{index}]-[FilterES] at [{DateTime.Now}]:\n{json}", logPath);
            }

            var searchResponse = _client.Search<personDTO>(searchDescriptor);

            var persons = searchResponse.Documents;
            if (searchResponse.IsValid)
            {
                data = persons.ToArray();
                return true;
            }
            else
            {
                errorMessages = errorMessages.Append(searchResponse.OriginalException.Message).ToArray();
                return false;
            }
        }
        public bool FilterDocumentES(documentDTO filter, out IEnumerable<documentDTO> data, out string[] errorMessages, bool fuzzy = false, int page = 1, int size = 10)
        {
            _ = Array.Empty<string>();
            data = Array.Empty<documentDTO>();
            if(DocumentToDict(filter, out IDictionary<string, object> dict, out errorMessages))
            {
                if (FilterES(dict, out personDTO[] personDatas, out errorMessages, fuzzy, page, size))
                {
                    //Console.Write(JsonSerializer.Serialize(personDatas));
                    foreach (var p in personDatas)
                    {
                        data = data.Append(ModelToDocumentWithAttributes(p));
                    }
                    return true;
                }
                return false;
            }
            return false;
        }
        public void FindPersonByPinES(string iin, ref ServiceContext context, int page = 1, int size = 10)
        {
            if (FilterES(new Dictionary<string, object> { { "iin", iin } }, out personDTO[] data, out string[] errorMessages, false, page, size))
            {
                var res = data.FirstOrDefault();
                context["Result"] = res;
                context.SuccessFlag = res != null;
            }
            else
            {
                throw new ApplicationException(string.Join(", ", errorMessages));
            }
        }
        
        private static SemaphoreSlim semaphoreLog = new(1, 1);
        public static void WriteLog(string text, string pathToFile)
        {
            semaphoreLog.Wait();
            //lock (lockObj)
            {
                using (StreamWriter sw = new StreamWriter(pathToFile, true))
                {
                    sw.WriteLine(text);
                }
            }
            semaphoreLog.Release();
        }
    }

}
