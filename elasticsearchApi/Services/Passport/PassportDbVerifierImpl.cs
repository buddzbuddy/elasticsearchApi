using elasticsearchApi.Contracts;
using elasticsearchApi.Contracts.Passport;
using elasticsearchApi.Services.Exceptions;
using elasticsearchApi.Services.Exceptions.Passport;
using SqlKata.Execution;
using System.Data;

namespace elasticsearchApi.Services.Passport
{
    public class PassportDbVerifierImpl : IPassportDbVerifier
    {
        private readonly QueryFactory _db;
        public PassportDbVerifierImpl(QueryFactory db) {
            _db = db;
        }
        public void Verify(in string? passportno, IDbTransaction? transaction = null)
        {
            var result = _db.Query("Persons").Where("passportno", passportno).Count<int>(transaction: transaction);

            if(result > 0)
            {
                throw new PassportDuplicateException("Паспорт с таким номером уже существует в базе НРСЗ");
            }
        }
    }
}
