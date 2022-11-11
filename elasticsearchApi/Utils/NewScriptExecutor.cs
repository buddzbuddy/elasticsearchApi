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

namespace elasticsearchApi.Utils
{
    public static class Refs
    {

        public class RegionDistrictItem
        {
            public int RegionNo { get; set; }
            public int DistrictNo { get; set; }
        }
        public static List<RegionDistrictItem> RegionDistricts { get; set; } = new List<RegionDistrictItem>();
    }
    public class NewScriptExecutor
    {
        private AppSettings _appSettings;
        SqlConnection connection;
        SqlServerCompiler compiler = new();
        public NewScriptExecutor(AppSettings appSettings)
        {
            _appSettings = appSettings;
            connection = new SqlConnection(_appSettings.nrsz_temp_connection);
        }
        /*IAppServiceProvider InitProvider(string username, Guid userId)
        {
            var dataContextFactory = DataContextFactoryProvider.GetFactory();

            var dataContext = dataContextFactory.CreateMultiDc("DataContexts");
            BaseServiceFactory.CreateBaseServiceFactories();
            var providerFactory = AppServiceProviderFactoryProvider.GetFactory();
            var provider = providerFactory.Create(dataContext);
            var serviceRegistrator = provider.Get<IAppServiceProviderRegistrator>();
            serviceRegistrator.AddService(new UserDataProvider(userId, username));
            return provider;
        }

        public WorkflowContext CreateContext(string username, Guid? userId)
        {
            if(userId == null)
            {
                userId = new Guid("{DCED7BEA-8A93-4BAF-964B-232E75A758C5}");
            }
            return new WorkflowContext(new WorkflowContextData(Guid.Empty, userId.Value), InitProvider(username, userId.Value));
        }*/

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

        
        public ServiceContext FindSamePerson(Person person)
        {
            //WorkflowContext context = CreateContext("asist2nrsz", new Guid("{05EEF54F-5BFE-4E2B-82C7-6AB6CD59D488}"));
            ServiceContext context = new ServiceContext();
            
            context.SuccessFlag = context.ErrorMessages == null || context.ErrorMessages.Count == 0;
            //verifyData(context, person);
            if (context.SuccessFlag)
            {
                var passportType = person.PassportType;
                var passportSeries = (string)person.PassportSeries ?? String.Empty;
                var hasPassportSeries = !String.IsNullOrEmpty(passportSeries);

                var passportNo = (string)person.PassportNo ?? String.Empty;
                var hasPassportNo = !String.IsNullOrEmpty(passportNo);

                
                var db = new QueryFactory(connection, compiler);

                var query = db.Query("Persons");

                if (!String.IsNullOrEmpty(person.IIN)) _ = query.Where("IIN", person.IIN); //qb.Where("IIN").Eq(person.IIN);
                if (!String.IsNullOrEmpty(person.SIN)) _ = query.Where("SIN", person.SIN); //qb.Where("SIN").Eq(person.SIN);

                if ((passportType != null) && hasPassportNo && hasPassportSeries)
                {
                    _ = query.Where("PassportType", passportType)
                        .Where("PassportSeries", passportSeries)
                        .Where("PassportNo",passportNo);
                }
                _ = query.Where("Last_Name", person.Last_Name).Where("First_Name", person.First_Name)
                    .Where("Sex", person.Sex).Where("Date_of_Birth", person.Date_of_Birth);
                if (!String.IsNullOrEmpty(person.Middle_Name))
                    _ = query.Where("Middle_Name", person.Middle_Name);
                //TODO: Use NATIVE SQL COMMAND

                var dupPersonList = query.Get();
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
            return context;
        }
        public ServiceContext FindSamePerson2(SearchPersonModel person)
        {
            //WorkflowContext context = CreateContext("asist2nrsz", new Guid("{05EEF54F-5BFE-4E2B-82C7-6AB6CD59D488}"));
            ServiceContext context = new();
            
            
            verifyData(context, person);
            context.SuccessFlag = context.ErrorMessages == null || context.ErrorMessages.Count == 0;
            if (context.SuccessFlag)
            {
                var settings = new ConnectionSettings(new Uri(_appSettings.host)).DefaultIndex(_appSettings.nrsz_persons_index_name);
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

        public ServiceContext FindPersons(Person person)
        {
            ServiceContext context = new ServiceContext();

            context.SuccessFlag = context.ErrorMessages == null || context.ErrorMessages.Count == 0;
            //verifyData(context, person);
            if (context.SuccessFlag)
            {
                var passportType = person.PassportType;
                var passportSeries = (string)person.PassportSeries ?? String.Empty;
                var hasPassportSeries = !String.IsNullOrEmpty(passportSeries);

                var passportNo = (string)person.PassportNo ?? String.Empty;
                var hasPassportNo = !String.IsNullOrEmpty(passportNo);

                var db = new QueryFactory(connection, compiler);
                var query = db.Query("Persons");

                if (!String.IsNullOrEmpty(person.IIN)) _ = query.Where("IIN", person.IIN); //qb.Where("IIN").Eq(person.IIN);
                if (!String.IsNullOrEmpty(person.SIN)) _ = query.Where("SIN", person.SIN); //qb.Where("SIN").Eq(person.SIN);

                if ((passportType != null) && hasPassportNo && hasPassportSeries)
                {
                    _ = query.Where("PassportType", passportType)
                        .Where("PassportSeries", passportSeries)
                        .Where("PassportNo", passportNo);
                }
                _ = query.Where("Last_Name", person.Last_Name).Where("First_Name", person.First_Name)
                    .Where("Sex", person.Sex).Where("Date_of_Birth", person.Date_of_Birth);
                if (!String.IsNullOrEmpty(person.Middle_Name))
                    _ = query.Where("Middle_Name", person.Middle_Name);
                //TODO: Use NATIVE SQL COMMAND
                var personList = query.Get();
                context["Persons"] = personList;
            }
            return context;
        }

        public ServiceContext FindPersons2(SearchPersonModel person)
        {
            ServiceContext context = new ServiceContext();

            context.SuccessFlag = context.ErrorMessages == null || context.ErrorMessages.Count == 0;
            verifyData(context, person);
            if (context.SuccessFlag)
            {
                var settings = new ConnectionSettings(new Uri(_appSettings.host)).DefaultIndex(_appSettings.nrsz_persons_index_name);
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
                    context["Persons"] = persons;
                }
            }
            return context;
        }

        public ServiceContext FindPersonByPIN(Person person)
        {
            ServiceContext context = new ServiceContext();
            context.SuccessFlag = false;

            System.Text.RegularExpressions.Regex regex =
                new System.Text.RegularExpressions.Regex("[^0-9]");

            if (!String.IsNullOrEmpty(person.IIN))
            {
                if (person.IIN.Length > 14)
                    context.AddErrorMessage("IIN", "Ошибка в длине ПИН");
                if (regex.IsMatch(person.IIN))
                    context.AddErrorMessage("IIN", "Ошибка в формате номера ПИН! Должны быть только цифры");
                if(context.ErrorMessages.Count == 0)
                {
                    var db = new QueryFactory(connection, compiler);
                    var query = db.Query("Persons").Where("IIN", person.IIN);
                    var res = query.FirstOrDefault();
                    context["Result"] = res;
                    context.SuccessFlag = res != null;
                }
            }
            return context;
        }
        public ServiceContext FindPersonByPIN2(SearchPersonModel person)
        {
            ServiceContext context = new()
            {
                SuccessFlag = false
            };

            System.Text.RegularExpressions.Regex regex =
                new("[^0-9]");

            if (!string.IsNullOrEmpty(person.iin))
            {
                var settings = new ConnectionSettings(new Uri(_appSettings.host)).DefaultIndex(_appSettings.nrsz_persons_index_name);
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

        public void InitRegionDistricts()
        {
            if(Refs.RegionDistricts.Count == 0)
            {
                using (SqlConnection connection = new SqlConnection(_appSettings.cissa_data_connection))
                {
                    SqlCommand cmd = new SqlCommand();
                    cmd.CommandText = @"
SELECT
	[Area1].[Number] as regionNo,
	[District].[Number] as [districtNo]
FROM
	(SELECT
		d.Id,
		[a1].[Value] as [Number],
		[a2].[Value] as [Area]
	FROM
		Documents d WITH(NOLOCK)
		INNER JOIN Int_Attributes a1 WITH(NOLOCK) on (a1.Document_Id = d.Id and a1.Def_Id = '8f74f065-4632-4ef4-87e3-ae0df1d3cfca' and a1.Expired = '99991231' and [a1].[Value] > 0)
		LEFT OUTER JOIN Document_Attributes a2 WITH(NOLOCK) on (a2.Document_Id = d.Id and a2.Def_Id = 'e6600d4b-f03c-4cb9-a67a-8b400cdf6f69' and a2.Expired = '99991231')
	WHERE
		d.Def_Id = 'ba5d4276-6bfb-4180-9d4f-828e38e95601' AND
		([d].[Deleted] is null OR [d].[Deleted] = 0)
	) as [District]
	INNER JOIN (SELECT
		d.Id,
		[a1].[Value] as [Number]
	FROM
		Documents d WITH(NOLOCK)
		INNER JOIN Int_Attributes a1 WITH(NOLOCK) on (a1.Document_Id = d.Id and a1.Def_Id = '9f8a9e68-676f-4c1d-9359-0784df076491' and a1.Expired = '99991231' and [a1].[Value] > 0)
	WHERE
		d.Def_Id = '67a81660-8a8b-4d80-9199-734a618edd32'
	) as [Area] on [Area].Id = [District].[Area]
	INNER JOIN (SELECT
		d.Id,
		[a1].[Value] as [Number]
	FROM
		Documents d WITH(NOLOCK)
		LEFT OUTER JOIN Int_Attributes a1 WITH(NOLOCK) on (a1.Document_Id = d.Id and a1.Def_Id = '9f8a9e68-676f-4c1d-9359-0784df076491' and a1.Expired = '99991231')
	WHERE
		d.Def_Id = '67a81660-8a8b-4d80-9199-734a618edd32'
	) as [Area1] on [Area1].Id = [District].[Area]
";

                    cmd.CommandType = CommandType.Text;
                    cmd.Connection = connection;

                    connection.Open();
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                            Refs.RegionDistricts.Add(new Refs.RegionDistrictItem { RegionNo = reader.GetInt32(0), DistrictNo = reader.GetInt32(1) });
                    }

                    connection.Close();
                }
            }
        }
        internal ServiceContext AddNewPerson(Person person, int regionNo, int districtNo)
        {
            ServiceContext context = new ();
            
            //verifyData(context, person);
            if (context.ErrorMessages != null && context.ErrorMessages.Count > 0) return context;

            InitRegionDistricts();
            if (!Refs.RegionDistricts.Any(x => x.RegionNo == regionNo && x.DistrictNo == districtNo))
                throw new ApplicationException(string.Format("Номера области и района отсутствуют в справочнике: regionNo - {0}, districtNo - {1}", regionNo, districtNo));

            /*var districtCode = districtNo.ToString();
            while (districtCode.Length < 3) districtCode = '0' + districtCode;*/
            var regCode = regionNo * 1000 + districtNo;//regionNo.ToString() + districtCode;

            var db = new QueryFactory(connection, compiler);
            lock (PinLock)
            {
                var maxPin = db.Query("Persons").WhereLike("IIN", regCode + "__________").Max<long?>("IIN");
                //var no = 0;
                //ss = maxPin.ToString();
                /*if (!String.IsNullOrWhiteSpace(ss) && ss.Length > 4)
                {
                    var sTemp = ss.Substring(4);
                    no = int.Parse(sTemp.Substring(0, (sTemp.Length - 1)));
                }*/
                var newPin = (maxPin + 1).ToString();
                //while (newPinCounter.Length < 10) newPinCounter = '0' + newPinCounter;
                context["NewPIN"] = newPin;//startPin + newPinCounter;//14-ти значное число
                context.SuccessFlag = true;
                context["Result"] = person;
                //CalcControlSum(context);
                context["ResultPIN"] = newPin;//startPin + newPinCounter;
                //var newPin = startPin + newPinCounter;//context["ResultPIN"].ToString();


                person.IIN = newPin;

                int tempId = db.Query("Persons").InsertGetId<int>(new
                {
                    person.Date_of_Birth,
                    person.Date_of_Issue,
                    person.FamilyState,
                    person.First_Name,
                    person.IIN,
                    person.Issuing_Authority,
                    person.Last_Name,
                    person.Middle_Name,
                    person.PassportNo,
                    person.PassportSeries,
                    person.PassportType,
                    person.Sex,
                    person.SIN
                });
                if(tempId > 0) //Successfully created, create async job to copy in db NRSZ-DATA
                {
                    WriteLog($"[NRSZ-TEMP] saved at {DateTime.Now:HH:mm:ss.fff} newPin: {newPin} tempid:{tempId}");
                    person.Id = tempId;
                    //var ctx = CreateContext("asist2nrsz", new Guid("{05EEF54F-5BFE-4E2B-82C7-6AB6CD59D488}"));
                    WriteLog($"[API] has sent to [NRSZ-DATA] at {DateTime.Now:HH:mm:ss.fff} pin: {newPin}");
                    //Task.Run(() => SaveInNrszData(person, ctx));
                }

                //COMPLETE PROCESS
                //TOD: ASYNC Save person in NRSZ DB linked with Id
                return context;
            }
        }
        static void CalcControlSum(ServiceContext context)
        {
            var pin = context["NewPIN"];

            var s = pin != null ? pin.ToString() : "";
            /*if (s.Length > 13)
                s = s.Substring(0, 13);*/
            if (s.Length != 13)
                throw new ApplicationException("Неверная длина ПИН");

            var sum = 0;
            for (var i = 0; i <= 9; i++)
            {
                var ch = s[i];
                var dn = int.Parse(ch.ToString());
                if (i % 2 == 0)
                {
                    var n = dn * 2;
                    if (n > 9) n = n - 9;
                    sum += n;
                }
                else
                    sum += dn;
            }
            sum = 10 - (sum % 10);
            if (sum == 10) sum = 0;
            context["ControlDigit"] = sum;
            context["ResultPIN"] = s + sum.ToString();
        }

        /*private static void SaveInNrszData(Person nrszPerson, WorkflowContext context)
        {
            try
            {
                var person = context.Documents.New(NrszPersonDefId); //(Doc)context["Person"];
                person["Last_Name"] = nrszPerson.Last_Name;
                person["First_Name"] = nrszPerson.First_Name;
                person["Middle_Name"] = nrszPerson.Middle_Name;
                person["IIN"] = nrszPerson.IIN;
                person["SIN"] = nrszPerson.SIN;
                person["Sex"] = nrszPerson.Sex;
                person["Date_of_Birth"] = nrszPerson.Date_of_Birth;
                person["PassportType"] = nrszPerson.PassportType;
                person["PassportSeries"] = nrszPerson.PassportSeries;
                person["PassportNo"] = nrszPerson.PassportNo;
                person["Date_of_Issue"] = nrszPerson.Date_of_Issue;
                person["Issuing_Authority"] = nrszPerson.Issuing_Authority;
                person["FamilyState"] = nrszPerson.FamilyState;
                person["tempId"] = nrszPerson.Id;
                //await Task.Delay(5000);
                context.Documents.Save(person);
                WriteLog($"[NRSZ-DATA] PIN-{nrszPerson.IIN} saved at {DateTime.Now:HH:mm:ss.fff}");
            }
            catch (Exception e)
            {
                WriteLog(string.Format("[ERROR] Async Task-SaveInNrszData() {2} - Error: {0}; trace: {1}", e.Message, e.StackTrace, DateTime.Now.ToString("HH:mm:ss.fff")));
            }
        }*/

        public static readonly object PinLock = new Object();

        private static readonly Guid NrszPersonDefId = new Guid("{6052978A-1ECB-4F96-A16B-93548936AFC0}");
        
        /*public void AssignService(ScriptExecutor.AssignServiceRequest request)
        {
            WorkflowContext context = CreateContext("asist2nrsz", new Guid("{05EEF54F-5BFE-4E2B-82C7-6AB6CD59D488}"));
            
            var qb = new QueryBuilder(PersonDefId);
            qb.Where("IIN").Eq(request.pin);
            Guid personId;
            Guid districtId;
            Guid regionId;

            using (var query = context.CreateSqlQuery(qb.Def))
            {
                using (var reader = context.CreateSqlReader(query))
                {
                    if (!reader.Read())
                        throw new ApplicationException(
                            String.Format("Не могу зарегистрированить назначение. Гражданин с указанным ПИН \"{0}\" не найден!", request.pin));

                    personId = reader.Reader.GetGuid(0);
                }
            }
            qb = new QueryBuilder(RaionDefId);
            qb.Where("Number").Eq(request.raionNo).And("Area").Include("Number").Eq(request.oblastNo);

            using (var query = context.CreateSqlQuery(qb.Def))
            {
                query.AddAttribute("&Id");
                query.AddAttribute(query.Sources[0], "&Id");
                using (var reader = context.CreateSqlReader(query))
                {
                    if (!reader.Read())
                        throw new ApplicationException("Не могу зарегистрировать назначение. Ошибка в коде области или района!");

                    districtId = reader.Reader.GetGuid(0);
                    regionId = reader.Reader.GetGuid(1);
                }
            }
            var docRepo = context.Documents;
            var assignedService = docRepo.New(AssignedServiceDefId);
            assignedService["Person"] = personId;
            assignedService["DateFrom"] = request.effectiveDate;
            assignedService["DateTo"] = request.expiryDate;
            if (request.amount > 0)
                assignedService["Amount"] = request.amount;
            assignedService["ServiceType"] = request.serviceTypeId;
            var userInfo = context.GetUserInfo();
            assignedService["AuthorityId"] = userInfo.OrganizationId;
            assignedService["District"] = districtId;
            assignedService["Area"] = regionId;
            assignedService["Djamoat"] = request.djamoat;
            assignedService["Village"] = request.village;
            assignedService["Street"] = request.street;
            assignedService["House"] = request.house;
            assignedService["Flat"] = request.flat;

            assignedService["DisabilityGroup"] = request.disabilityGroup;

            assignedService["RegDate"] = context["RegDate"];

            context.Documents.Save(assignedService);
        }*/

        public static readonly Guid PersonDefId = new Guid("{6052978A-1ECB-4F96-A16B-93548936AFC0}");
        public static readonly Guid ServiceTypeEnumDefId = new Guid("{EA5A7FC9-19AF-4E18-BF21-E8EE29D585C7}");
        public static readonly Guid TsaBenefitEnumId = new Guid("{C5C95DC9-CEFE-46F5-B6AA-4D23E5CE1008}"); // Пособие на АСП
        public static readonly Guid TsaPoorStatusEnumId = new Guid("{371B3E58-C039-4F8C-A299-B62666C23AB6}"); // Статус малообеспеченной семьи
        public static readonly Guid TsaDeadBenefitEnumId = new Guid("{2A5FA716-7D3A-467B-B7C8-C1E68251E7D4}"); // Пособие на погребение
        public static readonly Guid AssignedServiceDefId = new Guid("{A16EE2A1-CFDF-4B7A-8A32-28CC094C3486}");
        private static readonly Guid RaionDefId = new Guid("{BA5D4276-6BFB-4180-9D4F-828E38E95601}");
        private static readonly Guid RegionDefId = new Guid("{67A81660-8A8B-4D80-9199-734A618EDD32}");
        private static readonly Guid AssignedServicePaymentDefId = new Guid("{B9CB0BD2-9BD5-4F91-AD12-94B9FA6FC21D}");
        //private static readonly Guid AddressDefId = new Guid("{1C2DD4D9-73B5-4BB7-9B33-1D715B0773CE}");



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
