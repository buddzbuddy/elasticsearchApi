using elasticsearchApi.Contracts.Passport;
using elasticsearchApi.Models.Exceptions.Passport;
using elasticsearchApi.Models.Infrastructure;
using SqlKata.Execution;
using System;

namespace elasticsearchApi.Services.Passport
{
    public class ExistingPassportVerifierImpl : IExistingPassportVerifier
    {
        private readonly QueryFactory _queryFactory;
        private readonly AppTransaction _appTransaction;
        public ExistingPassportVerifierImpl(QueryFactory queryFactory, AppTransaction appTransaction)
        {
            _queryFactory = queryFactory;
            _appTransaction = appTransaction;
        }
        public void CheckExistingPassportByNo(string passportNo, int? excludePersonId = null)
        {
            var query = _queryFactory.Query("Persons").Where("PassportNo", passportNo);
            if(excludePersonId != null && excludePersonId > 0)
                query = query.WhereNot("Id", excludePersonId);
            var existingPersonByPassport = query.FirstOrDefault(_appTransaction.Transaction);
            if (existingPersonByPassport != null)
            {
                var msg = $"Найден дубликат по паспорту! Данный номер ({passportNo}) паспорта принадлежит существующему гражданину ПИН: {existingPersonByPassport.IIN}, ФИО: {existingPersonByPassport.Last_Name} {existingPersonByPassport.First_Name} {existingPersonByPassport.Middle_Name}, Номер паспорта: {existingPersonByPassport.PassportNo}, Дата рождения {existingPersonByPassport.Date_of_Birth}";
                throw new PassportDuplicateException(msg);
            }
        }
    }
}
