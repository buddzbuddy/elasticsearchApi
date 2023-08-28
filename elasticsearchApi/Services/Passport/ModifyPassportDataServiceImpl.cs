using elasticsearchApi.Contracts.Passport;
using elasticsearchApi.Models.Contracts;
using elasticsearchApi.Models.Exceptions.Passport;
using elasticsearchApi.Models.Exceptions.Person;
using elasticsearchApi.Models.Infrastructure;
using elasticsearchApi.Models.Passport;
using elasticsearchApi.Models.Person;
using SqlKata.Execution;
using System.Data;

namespace elasticsearchApi.Services.Passport
{
    public class ModifyPassportDataServiceImpl : IModifyPassportDataService
    {
        private readonly QueryFactory _queryFactory;
        private readonly IPassportVerifier _passportVerifier;
        private readonly AppTransaction _appTransaction;
        private readonly IExistingPassportVerifier _existingPassportVerifier;
        public ModifyPassportDataServiceImpl(QueryFactory queryFactory, IPassportVerifier passportVerifier,
            AppTransaction appTransaction, IExistingPassportVerifier existingPassportVerifier)
        {
            _passportVerifier = passportVerifier;
            _queryFactory = queryFactory;
            _appTransaction = appTransaction;
            _existingPassportVerifier= existingPassportVerifier;
        }
        public void Execute(string iin, modifyPersonPassportDTO person, outPersonDTO? personFullData = null)
        {
            _passportVerifier.VerifyPassport(person);

            var query = _queryFactory.Query("Persons").Where("IIN", iin).Where("deleted", 0);
            var existingPersonByIIN = query.FirstOrDefault(_appTransaction.Transaction);
            if (existingPersonByIIN == null)
            {
                throw new PersonNotFoundException("Гражданин с таким ПИН не найден в базе НРСЗ");
            }
            else
            {
                _existingPassportVerifier.CheckExistingPassportByNo(person.passportno, existingPersonByIIN.Id);

                if (_queryFactory.Connection.State != ConnectionState.Open)
                {
                    _queryFactory.Connection.Open();
                }
                _appTransaction.Transaction ??= _queryFactory.Connection.BeginTransaction();

                var d = existingPersonByIIN.Date_of_Issue;

                //PREPARE OUT DATA
                personFullData ??= new outPersonDTO();
                personFullData.date_of_birth = d;
                personFullData.iin = iin;
                personFullData.last_name = existingPersonByIIN.Last_Name;
                personFullData.first_name = existingPersonByIIN.First_Name;
                personFullData.middle_name = existingPersonByIIN.Middle_Name;
                personFullData.sex = existingPersonByIIN.Sex;

                var passportInsertObj = new
                {
                    PersonId = existingPersonByIIN.Id,
                    existingPersonByIIN.PassportType,
                    existingPersonByIIN.PassportSeries,
                    existingPersonByIIN.PassportNo,
                    Date_Of_Issue = d,
                    existingPersonByIIN.Issuing_Authority,
                    Marital_Status = existingPersonByIIN.FamilyState
                };
                

                /*var insertQuery = _db.Query("Passports").AsInsert(passportInsertObj);
                var sql = _db.Compiler.Compile(insertQuery).Sql;*/
                var affectedRows = _queryFactory.Query("Passports").Insert(passportInsertObj, _appTransaction.Transaction);
                if (affectedRows == 0)
                {
                    throw new PassportArchiveException("Архивация старых паспортных данных не записалась в историю");
                }
                else
                {
                    affectedRows = _queryFactory.Query("Persons").Where("Id", (int)existingPersonByIIN.Id).Update(person, _appTransaction.Transaction);
                    if (affectedRows == 0)
                    {
                        throw new PersonUpdateException("Паспортные данные гражданина в НРСЗ не обновлены");
                    }
                }
            }
        }
    }
}
