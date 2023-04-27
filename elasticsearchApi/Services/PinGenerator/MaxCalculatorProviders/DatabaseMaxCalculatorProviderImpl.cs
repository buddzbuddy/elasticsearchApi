using elasticsearchApi.Contracts.PinGenerator;
using SqlKata.Execution;

namespace elasticsearchApi.Services.PinGenerator.MaxCalculatorProviders
{
    public class DatabaseMaxCalculatorProviderImpl : IMaxCalculatorProvider
    {
        private readonly QueryFactory _queryFactory;
        public DatabaseMaxCalculatorProviderImpl(QueryFactory queryFactory)
        {
            _queryFactory = queryFactory;
        }

        public long CalculateMaxIIN(in int regCode)
        {
            long maxPin = _queryFactory.Query("Persons").WhereLike("IIN", regCode + "__________").Max<long?>("IIN") ?? 0;

            if (maxPin == 0) maxPin = regCode * 10000000000;

            return maxPin;
        }
    }
}
