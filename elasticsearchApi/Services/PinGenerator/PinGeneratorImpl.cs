using elasticsearchApi.Contracts.PinGenerator;
using Nest;
using SqlKata.Execution;

namespace elasticsearchApi.Services.PinGenerator
{
    public class PinGeneratorImpl : IPinGenerator
    {
        private readonly QueryFactory _queryFactory;

        public PinGeneratorImpl(QueryFactory queryFactory)
        {
            _queryFactory = queryFactory;
        }
        public string GenerateNewPin(in int regCode)
        {
            var maxPin = _queryFactory.Query("Persons").WhereLike("IIN", regCode + "__________").Max<long?>("IIN") ?? 0;

            if (maxPin == 0) maxPin = regCode * 10000000000;

            var newPin = (maxPin + 1).ToString();
            return newPin;
        }
    }
}
