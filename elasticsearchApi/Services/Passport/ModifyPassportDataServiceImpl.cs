using elasticsearchApi.Contracts.Passport;
using elasticsearchApi.Models;
using elasticsearchApi.Services.Exceptions;
using SqlKata.Execution;
using System.Data;

namespace elasticsearchApi.Services.Passport
{
    public class ModifyPassportDataServiceImpl : IModifyPassportDataService
    {
        private readonly QueryFactory _db;
        private readonly IPassportVerifier _passportVerifier;
        public ModifyPassportDataServiceImpl(QueryFactory db, IPassportVerifier passportVerifier) {
            _passportVerifier = passportVerifier;
            _db = db;
        }
        public void Execute(string iin, modifyPersonPassportDTO person, IDbTransaction? transaction)
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
            var personDb = query.FirstOrDefault(transaction);
            if (personDb == null)
            {
                throw new PersonNotFoundException("Гражданин с таким ПИН не найден в базе НРСЗ");
            }
            else
            {
                if (!string.IsNullOrEmpty(no) && !string.IsNullOrEmpty(series))
                {
                    query = _db.Query("Persons").Where("PassportType", passportType)
                        .Where("PassportSeries", series).Where("PassportNo", no)
                        .WhereNot("Id", personDb.Id);
                    var personDb2 = query.FirstOrDefault(transaction);
                    if (personDb2 != null)
                    {
                        var msg = $"Ошибка дублирования данных в документе \"{docTypeName}\"! ПИН: {personDb2.IIN}, ФИО: {personDb2.Last_Name} {personDb2.First_Name} {personDb2.Middle_Name}, \"{docTypeName}\" Номер: {personDb2.PassportNo}, Дата рождения {personDb2.Date_of_Birth}, found personIdByPIN: {personDb.Id}, found personIdByPassport: " + personDb2.Id;
                        throw new PassportDuplicateException(msg);
                    }
                }

                if(_db.Connection.State != ConnectionState.Open)
                {
                    _db.Connection.Open();
                }
                transaction = _db.Connection.BeginTransaction();

                var affectedRows = _db.Query("Passports").Insert(new
                {
                    PersonId = personDb.Id,
                    personDb.PassportType,
                    personDb.PassportSeries,
                    personDb.PassportNo,
                    personDb.Date_Of_Issue,
                    personDb.Issuing_Authority,
                    personDb.Marital_Status
                }, transaction);
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
                    }, transaction);
                    if (affectedRows == 0)
                    {
                        throw new PersonUpdateException("Паспортные данные гражданина в НРСЗ не обновлены");
                    }
                }
            }
        }
    }
}
