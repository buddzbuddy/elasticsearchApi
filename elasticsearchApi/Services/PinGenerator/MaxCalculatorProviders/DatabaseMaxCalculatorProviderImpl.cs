using elasticsearchApi.Contracts.PinGenerator;
using elasticsearchApi.Models.Infrastructure;
using SqlKata.Execution;

namespace elasticsearchApi.Services.PinGenerator.MaxCalculatorProviders
{
    public class DatabaseMaxCalculatorProviderImpl : IMaxCalculatorProvider
    {
        private readonly QueryFactory _queryFactory;
        private readonly AppTransaction _appTransaction;
        public DatabaseMaxCalculatorProviderImpl(QueryFactory queryFactory, AppTransaction appTransaction)
        {
            _queryFactory = queryFactory;
            _appTransaction = appTransaction;
        }

        public long CalculateMaxIIN(in int regCode)
        {
            long maxPin = _queryFactory.Query("Persons").WhereLike("IIN", regCode + "__________").Max<long?>("IIN", _appTransaction.Transaction) ?? 0;

            if (maxPin == 0) maxPin = regCode * 10000000000;

            return maxPin;
        }
    }
}
