namespace elasticsearchApi.Contracts.PinGenerator
{
    public interface IMaxCalculatorProvider
    {
        long CalculateMaxIIN(in int regCode);
    }
}
