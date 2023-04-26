using elasticsearchApi.Contracts.DataProviders;
using elasticsearchApi.Contracts;
using elasticsearchApi.Models.Person;
using elasticsearchApi.Services.DataProviders;
using elasticsearchApi.Services;
using elasticsearchApi.Tests.Helpers;
using elasticsearchApi.Tests.Infrastructure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit.Abstractions;
using FluentAssertions;
using elasticsearchApi.Contracts.CheckProviders;
using elasticsearchApi.Services.CheckExisting.Providers;
using Microsoft.Extensions.DependencyInjection;

namespace elasticsearchApi.Tests.Systems.Services
{
    public class TestCheckProviderMemory : TestUtils
    {
        public TestCheckProviderMemory(ITestOutputHelper output) : base(output)
        {
        }

        [Fact]
        public void Fetch_Durable_2SEC_WhenCalled_ReturnsData()
        {
            //Arrange
            var application = ApplicationHelper.GetWebApplication();
            using var services = application.Services.CreateScope();
            var cacheSvc1 = services.ServiceProvider.GetRequiredService<ICacheService>();
            var cacheSvc2 = services.ServiceProvider.GetRequiredService<ICacheService>();

            int cache_lifetime_secs = 2;

            var personDto = new outPersonDTO
            {
                last_name = "last_name1",
                first_name = "first_name1"
            };
            var filter = BaseService.ModelToDict(personDto);

            IInMemoryProvider inMemProvider = new InMemoryProviderImpl(cacheSvc1);
            inMemProvider.Save(personDto, cache_lifetime_secs);

            ICheckProvider sut = new CheckProviderMemoryImpl(inMemProvider);
            
            //Act & Assert

            var result = sut.FetchData(filter);
            result.Should().NotBeNullOrEmpty();

            Task.Delay(cache_lifetime_secs * 1000).Wait();

            result = sut.FetchData(filter);
            result.Should().BeNullOrEmpty();
        }
    }
}
