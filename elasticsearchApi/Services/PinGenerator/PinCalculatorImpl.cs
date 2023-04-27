using elasticsearchApi.Contracts.PinGenerator;

namespace elasticsearchApi.Services.PinGenerator
{
    public class PinCalculatorImpl : IPinCalculator
    {
        public long CalculateMaxIIN(in int regCode, IMaxCalculatorProvider calculatorProvider)
        {
            return calculatorProvider.CalculateMaxIIN(regCode);
        }
    }
}
