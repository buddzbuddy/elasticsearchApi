using elasticsearchApi.Contracts.Infrastructure;
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
    public class DatabaseMaxCalculatorProviderTests : TestUtils
    {
        public DatabaseMaxCalculatorProviderTests(ITestOutputHelper output) : base(output)
        {
        }


        [Fact]
        public void CalculateMaxIIN_WhenInvoked_Returns_IIN()
        {
            //Arrange
            var application = ApplicationHelper.GetWebApplication();
            using var services = application.Services.CreateScope();
            var addressRefsVerifier = services.ServiceProvider.GetRequiredService<IAddressRefsVerifier>();
            var sut = services.ServiceProvider.GetRequiredService<DatabaseMaxCalculatorProviderImpl>();

            int regionNo = 3, districtNo = 27;
            addressRefsVerifier.Verify(regionNo, districtNo);
            int regCode = regionNo * 1000 + districtNo;

            //Act
            var result = sut.CalculateMaxIIN(in regCode);

            //Assert
            result.Should().BeGreaterThan(0);
            _output.WriteLine(result.ToString());
        }
    }
}
