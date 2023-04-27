using elasticsearchApi.Contracts.Infrastructure;
using elasticsearchApi.Contracts.PinGenerator;
using elasticsearchApi.Services.PinGenerator.MaxCalculatorProviders;
using elasticsearchApi.Tests.Helpers;
using elasticsearchApi.Tests.Infrastructure;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit.Abstractions;

namespace elasticsearchApi.Tests.Systems.Services
{
    public class TestPinCalculator : TestUtils
    {
        public TestPinCalculator(ITestOutputHelper output) : base(output)
        {
        }

        [Fact]
        public void CalculateMaxIIN_Database_WhenCalled_Returns_MAX_IIN()
        {
            //Arrange
            var application = ApplicationHelper.GetWebApplication();
            using var services = application.Services.CreateScope();
            var addressRefsVerifier = services.ServiceProvider.GetRequiredService<IAddressRefsVerifier>();
            var databaseMaxCalculator = services.ServiceProvider.GetRequiredService<DatabaseMaxCalculatorProviderImpl>();
            var sut = services.ServiceProvider.GetRequiredService<IPinCalculator>();

            int regionNo = 3, districtNo = 27;
            addressRefsVerifier.Verify(regionNo, districtNo);
            int regCode = regionNo * 1000 + districtNo;

            //Act
            var result = sut.CalculateMaxIIN(in regCode, databaseMaxCalculator);

            //Assert
            result.ToString().Length.Should().Be(14);
            _output.WriteLine(result.ToString());
        }
    }
}
