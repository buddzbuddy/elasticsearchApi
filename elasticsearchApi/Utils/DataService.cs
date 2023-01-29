using AutoMapper;
using elasticsearchApi.Models;
using Humanizer;
using Microsoft.AspNetCore.Http.Extensions;
using Nest;
using SqlKata;
using SqlKata.Compilers;
using SqlKata.Execution;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.PortableExecutable;
using System.Threading;

namespace elasticsearchApi.Utils
{
    public interface IDataService
    {
        void AddNewPerson(addNewPersonDTO person, int regionNo, int districtNo, ref IServiceContext context);
        void ModifyPersonData(string iin, modifyPersonDataDTO person, ref IServiceContext context);
        void ModifyPersonPassport(string iin, modifyPersonPassportDTO person, ref IServiceContext context);
    }
    public class DataService : BaseService, IDataService
    {
        private readonly QueryFactory _db;
        private readonly IMapper _mapper;
        private readonly AppSettings _appSettings;
        private readonly IElasticService _es;

        public DataService(QueryFactory db, IMapper mapper, AppSettings appSettings, IElasticService es)
        {
            _db = db;
            _mapper = mapper;
            _appSettings= appSettings;
            _es = es;
        }

        private static readonly ConcurrentDictionary<int, Lazy<SemaphoreSlim>> _semaphore = new();
        private static void verifyFull(ref IServiceContext context, addNewPersonDTO person)
        {
            var passportTypeId = new Guid("{A77C7DB9-C27F-4FFC-BFC0-0C6959731B98}");
            var birthCertificateId = new Guid("{A52BE3AF-5DFA-405B-A4E6-18A64C24F9A5}");
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
            var series = v != null ? v.ToString().Trim() : String.Empty;
            if (isPassport && String.IsNullOrWhiteSpace(series))
                context.AddErrorMessage("passportseries", "Серия удостоверяющего документа не указана!");
            v = person.passportno;
            var no = v != null ? v.ToString().Trim() : String.Empty;
            if (isPassport && String.IsNullOrWhiteSpace(no))
                context.AddErrorMessage("passportno", "Номер удостоверяющего документа не указан!");
            var pDate = person.date_of_issue;
            if (isPassport && pDate == null)
                context.AddErrorMessage("date_of_issue", "Дата выдачи удостоверяющего документа не указана!");
            var issueDate = pDate;//!string.IsNullOrEmpty(v) ? Convert.ToDateTime(v) : (DateTime?)null;
            v = person.issuing_authority;
            var authority = v != null ? v.ToString().Trim() : String.Empty;
            if (isPassport && String.IsNullOrWhiteSpace(authority))
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
            s = iin;
            var pin = (s != null ? s.ToString() : String.Empty).Trim().ToLower();

            if (!String.IsNullOrEmpty(pin))
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

            if(_es.FilterES(ModelToDict(person), out outPersonDTO[] es_data, out string[] errorMessages))
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
            if (!Refs.RegionDistricts.Any(x => x.RegionNo == regionNo && x.DistrictNo == districtNo))
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
                    var result = _mapper.Map<outPersonDTO>(person);
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
        public void ModifyPersonPassport(string iin, modifyPersonPassportDTO person, ref IServiceContext context)

        {
            var passportTypeId = new Guid("{A77C7DB9-C27F-4FFC-BFC0-0C6959731B98}");
            var birthCertificateId = new Guid("{A52BE3AF-5DFA-405B-A4E6-18A64C24F9A5}");
            //HERE IS MORE COMPLEX
            System.Text.RegularExpressions.Regex nameRegex =
                    new System.Text.RegularExpressions.Regex("[0-9]");
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
            var series = v != null ? v.ToString().Trim() : String.Empty;
            if (isPassport && String.IsNullOrWhiteSpace(series))
                context.AddErrorMessage("passportseries", "Серия удостоверяющего документа не указана!");
            v = person.passportno;
            var no = v != null ? v.ToString().Trim() : String.Empty;
            if (isPassport && String.IsNullOrWhiteSpace(no))
                context.AddErrorMessage("passportno", "Номер удостоверяющего документа не указан!");
            var pDate = person.date_of_issue;
            if (isPassport && pDate == null)
                context.AddErrorMessage("date_of_issue", "Дата выдачи удостоверяющего документа не указана!");
            var issueDate = pDate;//!string.IsNullOrEmpty(v) ? Convert.ToDateTime(v) : (DateTime?)null;
            v = person.issuing_authority;
            var authority = v != null ? v.ToString().Trim() : String.Empty;
            if (isPassport && String.IsNullOrWhiteSpace(authority))
                context.AddErrorMessage("issuing_authority", "Орган выдавший удостоверяющий документ не указан!");
            var familyState = person.familystate;
            if (isPassport && familyState == null)
                context.AddErrorMessage("familystate", "Семейное положение не указано!");
            //}

            var query = _db.Query("Persons").Where("IIN", iin);
            var personDb = query.FirstOrDefault();
            if (personDb == null)
                context.AddErrorMessage("", "Гражданин с указанным ПИН не найден!");
            else
            {
                if (!String.IsNullOrEmpty(no) && !String.IsNullOrEmpty(series))
                {
                    query = _db.Query("Persons").Where("PassportType", passportType)
                        .Where("PassportSeries", series).Where("PassportNo", no)
                        .WhereNot("Id", personDb.Id);
                    var personDb2 = query.FirstOrDefault();
                    if (personDb2 != null)
                    {
                        var msg = $"Ошибка дублирования данных в документе \"{docTypeName}\"! ПИН: {personDb2.IIN}, ФИО: {personDb2.Last_Name} {personDb2.First_Name} {personDb2.Middle_Name}, \"{docTypeName}\" Номер: {personDb2.PassportNo}, Дата рождения {personDb2.Date_of_Birth}, found personIdByPIN: {personDb.Id}, found personIdByPassport: " + personDb2.Id;
                        context.AddErrorMessage("", msg);
                    }
                }

                if (personDb.PassportSeries != null || personDb.PassportNo != null)
                {
                    //_db.Statement("BEGIN TRANSACTION;");
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
                        context.AddErrorMessage("", "Паспортные данные не обновлены 1");
                        //_db.Statement("ROLLBACK TRANSACTION;");
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
                            context.AddErrorMessage("", "Паспортные данные не обновлены 1");
                            //_db.Statement("ROLLBACK TRANSACTION;");
                        }
                        /*else
                            _db.Statement("COMMIT TRANSACTION;");*/
                    }
                }
                else
                    context.AddErrorMessage("", "Паспортные данные не переданы");
            }
            context.SuccessFlag = context.ErrorMessages.Count == 0;
        }
    }
}
