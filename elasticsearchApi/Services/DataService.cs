using AutoMapper;
using elasticsearchApi.Contracts;
using elasticsearchApi.Data.Entities;
using elasticsearchApi.Models;
using elasticsearchApi.Utils;
using SqlKata.Execution;
using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading;

namespace elasticsearchApi.Services
{
    public interface IDataService
    {
        void AddNewPerson(addNewPersonDTO person, int regionNo, int districtNo, ref IServiceContext context);
        void ModifyPersonData(string iin, modifyPersonDataDTO person, ref IServiceContext context);
        ModifyPersonPassportResult ModifyPersonPassport(string iin, modifyPersonPassportDTO person, ref IServiceContext context);
    }
    public class DataService : BaseService, IDataService
    {
        private readonly QueryFactory _db;
        private readonly AppSettings _appSettings;
        private readonly IElasticService _es;
        private readonly ICacheService _cache;
        private readonly IPassportVerifier _passportVerifier;

        public DataService(QueryFactory db, AppSettings appSettings, IElasticService es, ICacheService cache, IPassportVerifier passportVerifier)
        {
            _db = db;
            _appSettings = appSettings;
            _es = es;
            _cache = cache;
            _passportVerifier = passportVerifier;
        }

        private static readonly ConcurrentDictionary<int, Lazy<SemaphoreSlim>> _semaphore = new();
        private static void verifyFull(ref IServiceContext context, addNewPersonDTO person)
        {
            var passportTypeId = new Guid("{A77C7DB9-C27F-4FFC-BFC0-0C6959731B98}");
            var birthCertificateId = new Guid("{A52BE3AF-5DFA-405B-A4E6-18A64C24F9A5}");
            System.Text.RegularExpressions.Regex nameRegex =
                new System.Text.RegularExpressions.Regex("[0-9]");
            object s = person.last_name;
            string lastName = (s != null ? s.ToString() : string.Empty).Trim().ToLower();

            if (string.IsNullOrEmpty(lastName)) //message = "Заполните фамилию!\n";
                context.AddErrorMessage("Last_Name", "Заполните фамилию");
            else
            {
                lastName = char.ToUpper(lastName[0]) + lastName.Substring(1);
                person.last_name = lastName;
                if (nameRegex.IsMatch(lastName))
                    context.AddErrorMessage("Last_Name", "Ошибка в формате фамилии! Должны быть только буквы.");
            }

            s = person.first_name;
            var firstName = (s != null ? s.ToString() : string.Empty).Trim().ToLower();

            if (string.IsNullOrEmpty(firstName))
                context.AddErrorMessage("First_Name", "Заполните имя");
            else
            {
                firstName = char.ToUpper(firstName[0]) + firstName.Substring(1);
                person.first_name = firstName;
                if (nameRegex.IsMatch(firstName))
                    context.AddErrorMessage("First_Name", "Ошибка в формате имени! Должны быть только буквы.");
            }

            s = person.middle_name;
            var middleName = (s != null ? s.ToString() : string.Empty).Trim().ToLower();

            if (!string.IsNullOrEmpty(middleName))
            {
                middleName = char.ToUpper(middleName[0]) + middleName.Substring(1);
                person.middle_name = middleName;
                if (nameRegex.IsMatch(middleName))
                    context.AddErrorMessage("Middle_Name", "Ошибка в формате отчества! Должны быть только буквы.");
            }

            System.Text.RegularExpressions.Regex regex =
                new System.Text.RegularExpressions.Regex("[^0-9]");
            s = person.iin;
            var pin = (s != null ? s.ToString() : string.Empty).Trim().ToLower();

            if (!string.IsNullOrEmpty(pin))
            {
                if (pin.Length > 14)
                    context.AddErrorMessage("IIN", "Ошибка в длине ПИН");
                if (regex.IsMatch(pin))
                    context.AddErrorMessage("IIN", "Ошибка в формате номера ПИН! Должны быть только цифры");
            }


            s = person.sex;
            if (s == null) //message += "Введите пол!\n"; 
                context.AddErrorMessage("Sex", "Введите пол");

            s = person.date_of_birth;
            if (s == null) //message += "Введите дату рождения!\n"; 
                context.AddErrorMessage("Date_of_Birth", "Введите дату рождения");

            //PASSPORT VERIFY
            var passportType = person.passporttype;
            var docTypeName = "";
            if ((passportType ?? Guid.Empty) != Guid.Empty)
            {
                docTypeName = passportType.ToString();//context.Enums.GetValue((Guid)passportType).Value;
            }
            if (passportType == null)
                context.AddErrorMessage("PassportType", "Тип удостоверяющего документа не указан!");
            var isPassport = passportType != null && (Guid)passportType == passportTypeId;
            //{
            var v = person.passportseries;
            var series = v != null ? v.ToString().Trim() : string.Empty;
            if (isPassport && string.IsNullOrWhiteSpace(series))
                context.AddErrorMessage("passportseries", "Серия удостоверяющего документа не указана!");
            v = person.passportno;
            var no = v != null ? v.ToString().Trim() : string.Empty;
            if (isPassport && string.IsNullOrWhiteSpace(no))
                context.AddErrorMessage("passportno", "Номер удостоверяющего документа не указан!");
            var pDate = person.date_of_issue;
            if (isPassport && pDate == null)
                context.AddErrorMessage("date_of_issue", "Дата выдачи удостоверяющего документа не указана!");
            var issueDate = pDate;//!string.IsNullOrEmpty(v) ? Convert.ToDateTime(v) : (DateTime?)null;
            v = person.issuing_authority;
            var authority = v != null ? v.ToString().Trim() : string.Empty;
            if (isPassport && string.IsNullOrWhiteSpace(authority))
                context.AddErrorMessage("issuing_authority", "Орган выдавший удостоверяющий документ не указан!");
            var familyState = person.familystate;
            if (isPassport && familyState == null)
                context.AddErrorMessage("familystate", "Семейное положение не указано!");

            context.SuccessFlag = context.ErrorMessages.Count == 0;
        }
        private static void verifyData(ref IServiceContext context, modifyPersonDataDTO person, string iin)
        {
            System.Text.RegularExpressions.Regex nameRegex =
                new System.Text.RegularExpressions.Regex("[0-9]");
            object s = person.last_name;
            string lastName = (s != null ? s.ToString() : string.Empty).Trim().ToLower();

            if (string.IsNullOrEmpty(lastName)) //message = "Заполните фамилию!\n";
                context.AddErrorMessage("Last_Name", "Заполните фамилию");
            else
            {
                lastName = char.ToUpper(lastName[0]) + lastName.Substring(1);
                person.last_name = lastName;
                if (nameRegex.IsMatch(lastName))
                    context.AddErrorMessage("Last_Name", "Ошибка в формате фамилии! Должны быть только буквы.");
            }

            s = person.first_name;
            var firstName = (s != null ? s.ToString() : string.Empty).Trim().ToLower();

            if (string.IsNullOrEmpty(firstName))
                context.AddErrorMessage("First_Name", "Заполните имя");
            else
            {
                firstName = char.ToUpper(firstName[0]) + firstName.Substring(1);
                person.first_name = firstName;
                if (nameRegex.IsMatch(firstName))
                    context.AddErrorMessage("First_Name", "Ошибка в формате имени! Должны быть только буквы.");
            }

            s = person.middle_name;
            var middleName = (s != null ? s.ToString() : string.Empty).Trim().ToLower();

            if (!string.IsNullOrEmpty(middleName))
            {
                middleName = char.ToUpper(middleName[0]) + middleName.Substring(1);
                person.middle_name = middleName;
                if (nameRegex.IsMatch(middleName))
                    context.AddErrorMessage("Middle_Name", "Ошибка в формате отчества! Должны быть только буквы.");
            }

            System.Text.RegularExpressions.Regex regex =
                new System.Text.RegularExpressions.Regex("[^0-9]");
            s = iin;
            var pin = (s != null ? s.ToString() : string.Empty).Trim().ToLower();

            if (!string.IsNullOrEmpty(pin))
            {
                if (pin.Length > 14)
                    context.AddErrorMessage("IIN", "Ошибка в длине ПИН");
                if (regex.IsMatch(pin))
                    context.AddErrorMessage("IIN", "Ошибка в формате номера ПИН! Должны быть только цифры");
            }
            else
                context.AddErrorMessage("IIN", "Пин пустой");

            s = person.sex;
            if (s == null) //message += "Введите пол!\n"; 
                context.AddErrorMessage("Sex", "Введите пол");

            s = person.date_of_birth;
            if (s == null) //message += "Введите дату рождения!\n"; 
                context.AddErrorMessage("Date_of_Birth", "Введите дату рождения");
            context.SuccessFlag = context.ErrorMessages.Count == 0;
        }

        public void AddNewPerson(addNewPersonDTO person, int regionNo, int districtNo, ref IServiceContext context)
        {
            verifyFull(ref context, person);
            if (!context.SuccessFlag) return;

            if (_es.FilterES(ModelToDict(person), out outPersonDTO[] es_data, out string[] errorMessages, out long totalCount))
            {
                if (es_data != null && es_data.Length > 0)
                {
                    //RETURN EXISTING PERSON
                    var result = es_data[0];
                    context["NewPIN"] = result.iin;
                    context["Result"] = result;
                    context["ResultPIN"] = result.iin;
                    context.SuccessFlag = true;
                    return;
                }
            }
            else
            {
                context.AddErrorMessage("ElasticSearch", $"Не смог проверить в базе НРСЗ: {errorMessages.ToStringJoin()}");
                context.SuccessFlag = false;
                return;
            }
            var addressRefs = _cache.GetObject(CacheKeys.ADDRESS_REFS_KEY);
            if (addressRefs == null)
            {
                context.AddErrorMessage("AddressRefs", $"Адресный справочник не загружен в память! Перезапустите и проверьте!");
                context.SuccessFlag = false;
                return;
            }
            if (!((AddressEntity[])addressRefs).Any(x => x.regionNo == regionNo && x.districtNo == districtNo))
            {
                context.AddErrorMessage("", string.Format("Номера области и района отсутствуют в справочнике: regionNo - {0}, districtNo - {1}", regionNo, districtNo));
                context.SuccessFlag = false;
                return;
            }

            var regCode = regionNo * 1000 + districtNo;
            var sem = _semaphore.GetOrAdd(regCode, _ => new Lazy<SemaphoreSlim>(() => new SemaphoreSlim(1, 1))).Value;
            sem.Wait();
            try
            {
                var maxPin = _db.Query("Persons").WhereLike("IIN", regCode + "__________").Max<long?>("IIN") ?? 0;

                if (maxPin == 0) maxPin = regCode * 10000000000;

                var newPin = (maxPin + 1).ToString();
                person.iin = newPin;

                int id = _db.Query("Persons").InsertGetId<int>(person);
                if (id > 0) //Successfully created, create async job to copy in db NRSZ-DATA
                {
                    context["NewPIN"] = newPin;
                    context.SuccessFlag = true;
                    var result = StaticReferences.InitInhertedProperties<addNewPersonDTO, outPersonDTO>(person);
                    result.id = id;
                    context["Result"] = result;
                    context["ResultPIN"] = newPin;
                }
                else
                {
                    var query = _db.Query("Persons").AsInsert(person);
                    var result = _db.Compiler.Compile(query);
                    WriteLog($"[{nameof(DataService)}]-[{nameof(AddNewPerson)}] at [{DateTime.Now}]:\n{result.Sql}\n{string.Join(",", result.Bindings)}", _appSettings.error_logpath);
                    context.SuccessFlag = false;
                    context.AddErrorMessage("", "NRSZ-TEMP is not created");
                }
            }
            catch (Exception e)
            {
                context.SuccessFlag = false;
                context.AddErrorMessage("", e.GetBaseException().Message);
                WriteLog($"[{nameof(DataService)}]-[{nameof(AddNewPerson)}] at [{DateTime.Now}]:\n{e.GetBaseException().Message}\n{e.StackTrace}", _appSettings.error_logpath);
            }
            finally
            {
                sem.Release();
            }
        }
        public void ModifyPersonData(string iin, modifyPersonDataDTO person, ref IServiceContext context)
        {
            verifyData(ref context, person, iin);
            if (!context.SuccessFlag) return;

            var query = _db.Query("Persons").Where("IIN", iin);
            int affectedRows = query.Update(person);
            if (affectedRows > 0) //Successfully created, create async job to copy in db NRSZ-DATA
            {
                context.SuccessFlag = true;
            }
            else
            {
                var result = _db.Compiler.Compile(query);
                WriteLog($"[{nameof(DataService)}]-[{nameof(ModifyPersonData)}] at [{DateTime.Now}]:\n{result.Sql}\n{string.Join(",", result.Bindings)}", _appSettings.error_logpath);
                context.SuccessFlag = false;
                context.AddErrorMessage("", "NRSZ-TEMP is not updated");
            }
        }
        public ModifyPersonPassportResult ModifyPersonPassport(string iin, modifyPersonPassportDTO person, ref IServiceContext context)
        {
            var passportType = person.passporttype;
            var docTypeName = "";
            if ((passportType ?? Guid.Empty) != Guid.Empty)
            {
                docTypeName = passportType.ToString();
            }
            
            var v = person.passportseries;
            var series = v != null ? v.ToString().Trim() : string.Empty;
            v = person.passportno;
            var no = v != null ? v.ToString().Trim() : string.Empty;
            var pDate = person.date_of_issue;
            var issueDate = pDate;
            v = person.issuing_authority;
            var authority = v != null ? v.ToString().Trim() : string.Empty;
            var familyState = person.familystate;
            
            _passportVerifier.VerifyPassport(person);

            var query = _db.Query("Persons").Where("IIN", iin);
            var personDb = query.FirstOrDefault();
            if (personDb == null)
            {
                context.AddErrorMessage("", ModifyPersonPassportResult.NRSZ_NOT_FOUND_BY_PIN.GetDescription());
                return ModifyPersonPassportResult.NRSZ_NOT_FOUND_BY_PIN;
            }
            else
            {
                if (!string.IsNullOrEmpty(no) && !string.IsNullOrEmpty(series))
                {
                    query = _db.Query("Persons").Where("PassportType", passportType)
                        .Where("PassportSeries", series).Where("PassportNo", no)
                        .WhereNot("Id", personDb.Id);
                    var personDb2 = query.FirstOrDefault();
                    if (personDb2 != null)
                    {
                        var msg = $"Ошибка дублирования данных в документе \"{docTypeName}\"! ПИН: {personDb2.IIN}, ФИО: {personDb2.Last_Name} {personDb2.First_Name} {personDb2.Middle_Name}, \"{docTypeName}\" Номер: {personDb2.PassportNo}, Дата рождения {personDb2.Date_of_Birth}, found personIdByPIN: {personDb.Id}, found personIdByPassport: " + personDb2.Id;
                        context.AddErrorMessage("", msg);
                        return ModifyPersonPassportResult.PASSPORT_DUPLICATE;
                    }
                }

                var affectedRows = _db.Query("Passports").Insert(new
                {
                    PersonId = personDb.Id,
                    personDb.PassportType,
                    personDb.PassportSeries,
                    personDb.PassportNo,
                    personDb.Date_Of_Issue,
                    personDb.Issuing_Authority,
                    personDb.Marital_Status
                });
                if (affectedRows == 0)
                {
                    var response = ModifyPersonPassportResult.PASSPORT_NOT_INSERTED_TO_ARCHIVE;
                    context.AddErrorMessage("", response.GetDescription());
                    return response;
                }
                else
                {
                    affectedRows = _db.Query("Persons").Where("Id", (int)personDb.Id).Update(new
                    {
                        PassportType = passportType,
                        PassportSeries = series,
                        PassportNo = no,
                        Date_of_Issue = issueDate,
                        Issuing_Authority = authority,
                        FamilyState = familyState
                    });
                    if (affectedRows == 0)
                    {
                        context.AddErrorMessage("", ModifyPersonPassportResult.PASSPORT_NOT_UPDATED.GetDescription());
                        return ModifyPersonPassportResult.PASSPORT_NOT_UPDATED;
                    }
                }
            }
            context.SuccessFlag = context.ErrorMessages.Count == 0;
            return ModifyPersonPassportResult.OK;
        }

        private bool verifyInputData(modifyPersonPassportDTO person, ref IServiceContext context)
        {
            var passportTypeId = new Guid("{A77C7DB9-C27F-4FFC-BFC0-0C6959731B98}");
            var birthCertificateId = new Guid("{A52BE3AF-5DFA-405B-A4E6-18A64C24F9A5}");
            //HERE IS MORE COMPLEX
            var passportType = person.passporttype;
            var docTypeName = "";
            if ((passportType ?? Guid.Empty) != Guid.Empty)
            {
                docTypeName = passportType.ToString();
            }
            if (passportType == null)
                context.AddErrorMessage("PassportType", "Тип удостоверяющего документа не указан!");
            var isPassport = passportType != null && (Guid)passportType == passportTypeId;

            var v = person.passportseries;
            var series = v != null ? v.ToString().Trim() : string.Empty;
            if (isPassport && string.IsNullOrWhiteSpace(series))
                context.AddErrorMessage("passportseries", "Серия удостоверяющего документа не указана!");
            v = person.passportno;
            var no = v != null ? v.ToString().Trim() : string.Empty;
            if (isPassport && string.IsNullOrWhiteSpace(no))
                context.AddErrorMessage("passportno", "Номер удостоверяющего документа не указан!");
            var pDate = person.date_of_issue;
            if (isPassport && pDate == null)
                context.AddErrorMessage("date_of_issue", "Дата выдачи удостоверяющего документа не указана!");
            var issueDate = pDate;
            v = person.issuing_authority;
            var authority = v != null ? v.ToString().Trim() : string.Empty;
            if (isPassport && string.IsNullOrWhiteSpace(authority))
                context.AddErrorMessage("issuing_authority", "Орган выдавший удостоверяющий документ не указан!");
            var familyState = person.familystate;
            if (isPassport && familyState == null)
                context.AddErrorMessage("familystate", "Семейное положение не указано!");
            return context.ErrorMessages.Count == 0;
        }
    }
}
