namespace elasticsearchApi.Contracts.PinGenerator
{
    public interface IPinCalculator
    {
        long CalculateMaxIIN(in int regCode, IMaxCalculatorProvider calculatorProvider);
    }
}
