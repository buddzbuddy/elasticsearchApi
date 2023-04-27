using elasticsearchApi.Contracts.Infrastructure;
using elasticsearchApi.Contracts.PinGenerator;
using elasticsearchApi.Models.Exceptions.Base;
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
    public class TestPinGenerator : TestUtils
    {
        public TestPinGenerator(ITestOutputHelper output) : base(output)
        {
        }

        [Fact]
        public void GenerateNewPin_Database_WhenCalled_Returns_New_PIN()
        {
            //Arrange
            var application = ApplicationHelper.GetWebApplication();
            using var services = application.Services.CreateScope();
            var addressRefsVerifier = services.ServiceProvider.GetRequiredService<IAddressRefsVerifier>();
            var databaseMaxCalculator = services.ServiceProvider.GetRequiredService<DatabaseMaxCalculatorProviderImpl>();
            var pinCalculator = services.ServiceProvider.GetRequiredService<IPinCalculator>();

            int regionNo = 3, districtNo = 27;
            addressRefsVerifier.Verify(regionNo, districtNo);
            int regCode = regionNo * 1000 + districtNo;
            var maxPin = pinCalculator.CalculateMaxIIN(in regCode, databaseMaxCalculator);

            var sut = services.ServiceProvider.GetRequiredService<IPinGenerator>();

            //Act
            var result = sut.GenerateNewPin(in regCode);

            //Assert
            maxPin.ToString().Length.Should().Be(14);
            result.Should().Be((maxPin + 1).ToString());
            _output.WriteLine($"maxPin:{maxPin} -> result: {result}");
        }

        [Fact]
        public void GenerateNewPin_WhenCalled_ThrowsAddessRefsException()
        {
            //Arrange
            var application = ApplicationHelper.GetWebApplication();
            using var services = application.Services.CreateScope();
            var addressRefsVerifier = services.ServiceProvider.GetRequiredService<IAddressRefsVerifier>();
            var databaseMaxCalculator = services.ServiceProvider.GetRequiredService<DatabaseMaxCalculatorProviderImpl>();
            var pinCalculator = services.ServiceProvider.GetRequiredService<IPinCalculator>();

            int regionNo = 30, districtNo = 27;
            //Act & Assert
            Assert.Throws<AddressRefsException>(() => addressRefsVerifier.Verify(regionNo, districtNo));
        }
    }
}
