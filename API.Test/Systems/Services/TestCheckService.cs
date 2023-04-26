using elasticsearchApi.Contracts.CheckProviders;
using elasticsearchApi.Contracts.DataProviders;
using elasticsearchApi.Models.Person;
using elasticsearchApi.Services;
using elasticsearchApi.Services.CheckExisting;
using elasticsearchApi.Services.CheckExisting.Providers;
using elasticsearchApi.Tests.Helpers;
using elasticsearchApi.Tests.Infrastructure;
using FluentAssertions;
using FluentAssertions.Common;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit.Abstractions;

namespace elasticsearchApi.Tests.Systems.Services
{
    public class TestCheckService : TestUtils
    {
        public TestCheckService(ITestOutputHelper output) : base(output)
        {
        }

        [Fact]
        public void CheckExisting_WhenCalled_Returns_Existing()
        {
            //Arrange
            var application = ApplicationHelper.GetWebApplication();
            using var services = application.Services.CreateScope();
            var inMemoryProvider = services.ServiceProvider.GetRequiredService<IInMemoryProvider>();
            ICheckProvider checkMemoryProvider = services.ServiceProvider.GetRequiredService<CheckProviderMemoryImpl>();
            
            ICheckService sut = new CheckServiceImpl();


            var personDto = new outPersonDTO
            {
                last_name = "last_name1",
                first_name = "first_name1"
            };
            var personDto2 = personDto.CreateDeepCopy();
            personDto2.middle_name = "middle_name2";
            inMemoryProvider.Save(personDto);

            var filter1 = BaseService.ModelToDict(personDto);
            var filter2 = BaseService.ModelToDict(personDto2);

            //Act
            var result1 = sut.CheckExisting(checkMemoryProvider, filter1);
            var result2 = sut.CheckExisting(checkMemoryProvider, filter2);

            var mockCheckElasticProvider = new Mock<CheckProviderElasticImpl>();
            mockCheckElasticProvider.Setup(svc => svc.FetchData(It.IsAny<IDictionary<string, object>>())).Returns(new[] { new outPersonDTO { id = 99 } });
            var result3 = sut.CheckExisting(mockCheckElasticProvider.Object, filter2);

            //Assert
            result1.Should().NotBeNull();
            result2.Should().BeNull();
            mockCheckElasticProvider.Verify(svc => svc.FetchData(It.IsAny<IDictionary<string, object>>()), Times.Once);
            result3.id.Should().Be(99);
        }
    }
}
