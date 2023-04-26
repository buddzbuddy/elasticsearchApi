using elasticsearchApi.Contracts.CheckProviders;
using elasticsearchApi.Contracts.DataProviders;
using elasticsearchApi.Models.Person;
using elasticsearchApi.Services.CheckExisting.Providers;
using elasticsearchApi.Services.CheckExisting;
using elasticsearchApi.Services;
using elasticsearchApi.Tests.Helpers;
using elasticsearchApi.Tests.Infrastructure;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit.Abstractions;
using Microsoft.Extensions.DependencyInjection;
using FluentAssertions;

namespace elasticsearchApi.Tests.Systems.Services
{
    public class TestCheckFacade : TestUtils
    {
        public TestCheckFacade(ITestOutputHelper output) : base(output)
        {
        }

        [Fact]
        public void CallCheck_WhenCalled_ReturnsData()
        {
            //Arrange
            var personDto = new outPersonDTO
            {
                last_name = "last_name1",
                first_name = "first_name1"
            };
            var personDto2 = personDto.CreateDeepCopy();
            personDto2.middle_name = "middle_name2";
            var personDto3 = personDto2.CreateDeepCopy();
            personDto3.iin = "iin";

            var filter1 = BaseService.ModelToDict(personDto);
            var filter2 = BaseService.ModelToDict(personDto2);
            var filter3 = BaseService.ModelToDict(personDto3);

            var application = ApplicationHelper.GetWebApplication();
            using var services = application.Services.CreateScope();
            var inMemoryProvider = services.ServiceProvider.GetRequiredService<IInMemoryProvider>();
            var checkMemoryProvider = services.ServiceProvider.GetRequiredService<CheckProviderMemoryImpl>();

            var mockCheckElasticProvider = new Mock<CheckProviderElasticImpl>();
            mockCheckElasticProvider.Setup(svc => svc.FetchData(filter3)).Returns(new[] { new outPersonDTO { id = 99 } });

            ICheckService checkSvc = services.ServiceProvider.GetRequiredService<ICheckService>();

            ICheckFacade sut = new CheckFacadeImpl(checkSvc, checkMemoryProvider, mockCheckElasticProvider.Object);


            inMemoryProvider.Save(personDto);

            

            //Act
            var result1 = sut.CallCheck(filter1);
            var result2 = sut.CallCheck(filter2);
            var result3 = sut.CallCheck(filter3);

            //Assert
            result1.Should().NotBeNull();
            result2.Should().BeNull();
            mockCheckElasticProvider.Verify(svc => svc.FetchData(filter3), Times.Once);
            result3.id.Should().Be(99);
        }
    }
}
