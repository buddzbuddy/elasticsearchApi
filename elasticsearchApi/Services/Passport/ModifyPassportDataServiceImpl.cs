using elasticsearchApi.Contracts.Passport;
using elasticsearchApi.Models.Infrastructure;
using elasticsearchApi.Models.Passport;
using elasticsearchApi.Services.Exceptions;
using elasticsearchApi.Services.Exceptions.Passport;
using elasticsearchApi.Services.Exceptions.Peron;
using SqlKata.Execution;
using System.Data;

namespace elasticsearchApi.Services.Passport
{
    public class ModifyPassportDataServiceImpl : IModifyPassportDataService
    {
        private readonly QueryFactory _db;
        private readonly IPassportVerifier _passportVerifier;
        private readonly AppTransaction _appTransaction;
        public ModifyPassportDataServiceImpl(QueryFactory db, IPassportVerifier passportVerifier, AppTransaction appTransaction)
        {
            _passportVerifier = passportVerifier;
            _db = db;
            _appTransaction = appTransaction;
        }
        public void Execute(string iin, modifyPersonPassportDTO person)
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
            var personDb = query.FirstOrDefault(_appTransaction.Transaction);
            if (personDb == null)
            {
                throw new PersonNotFoundException("Гражданин с таким ПИН не найден в базе НРСЗ");
            }
            else
            {
                if (!string.IsNullOrEmpty(no) && !string.IsNullOrEmpty(series))
                {
                    query = _db.Query("Persons")./*Where("PassportType", passportType)
                        .Where("PassportSeries", series).*/Where("PassportNo", no)
                        .WhereNot("Id", personDb.Id);
                    var personDb2 = query.FirstOrDefault(_appTransaction.Transaction);
                    if (personDb2 != null)
                    {
                        var msg = $"Ошибка дублирования данных в документе \"{docTypeName}\"! ПИН: {personDb2.IIN}, ФИО: {personDb2.Last_Name} {personDb2.First_Name} {personDb2.Middle_Name}, \"{docTypeName}\" Номер: {personDb2.PassportNo}, Дата рождения {personDb2.Date_of_Birth}, found personIdByPIN: {personDb.Id}, found personIdByPassport: " + personDb2.Id;
                        throw new PassportDuplicateException(msg);
                    }
                }

                if (_db.Connection.State != ConnectionState.Open)
                {
                    _db.Connection.Open();
                }
                _appTransaction.Transaction ??= _db.Connection.BeginTransaction();
                var d = personDb.Date_of_Issue;
                var passportInsertObj = new
                {
                    PersonId = personDb.Id,
                    personDb.PassportType,
                    personDb.PassportSeries,
                    personDb.PassportNo,
                    Date_Of_Issue = d,
                    personDb.Issuing_Authority,
                    Marital_Status = personDb.FamilyState
                };
                

                /*var insertQuery = _db.Query("Passports").AsInsert(passportInsertObj);
                var sql = _db.Compiler.Compile(insertQuery).Sql;*/
                var affectedRows = _db.Query("Passports").Insert(passportInsertObj, _appTransaction.Transaction);
                if (affectedRows == 0)
                {
                    throw new PassportArchiveException("Архивация старых паспортных данных не записалась в историю");
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
                    }, _appTransaction.Transaction);
                    if (affectedRows == 0)
                    {
                        throw new PersonUpdateException("Паспортные данные гражданина в НРСЗ не обновлены");
                    }
                }
            }
        }
    }
}
