using elasticsearchApi.Contracts.PinGenerator;
using elasticsearchApi.Services.PinGenerator.MaxCalculatorProviders;
using Nest;
using SqlKata.Execution;

namespace elasticsearchApi.Services.PinGenerator
{
    public class PinGeneratorImpl : IPinGenerator
    {
        private readonly IPinCalculator _pinCalculator;
        private readonly DatabaseMaxCalculatorProviderImpl _databaseMaxCalculator;
        public PinGeneratorImpl(IPinCalculator pinCalculator, DatabaseMaxCalculatorProviderImpl databaseMaxCalculator)
        {
            _pinCalculator = pinCalculator;
            _databaseMaxCalculator = databaseMaxCalculator;
        }
        public string GenerateNewPin(in int regCode)
        {
            long maxPin = _pinCalculator.CalculateMaxIIN(regCode, _databaseMaxCalculator);
            var newPin = (maxPin + 1).ToString();
            return newPin;
        }
    }
}
