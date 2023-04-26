using elasticsearchApi.Contracts;
using elasticsearchApi.Contracts.DataProviders;
using elasticsearchApi.Models.Person;
using elasticsearchApi.Services;
using elasticsearchApi.Services.DataProviders;
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
    public class TestInMemoryProvider : TestUtils
    {
        public TestInMemoryProvider(ITestOutputHelper output) : base(output)
        {
        }

        [Fact]
        public void Save_WhenCalled_SavesDataToMemory()
        {
            //Arrange
            var application = ApplicationHelper.GetWebApplication();
            using var services = application.Services.CreateScope();
            var cacheSvc = services.ServiceProvider.GetRequiredService<ICacheService>();

            var personDto = new outPersonDTO
            {
                last_name = "last_name1",
                first_name = "first_name1"
            };
            var filter = BaseService.ModelToDict(personDto);

            IInMemoryProvider sut = new InMemoryProviderImpl(cacheSvc);

            //Act
            sut.Save(personDto);

            //Assert
            var allPersons = sut.Fetch(filter);
            allPersons.Should().NotBeNullOrEmpty();
            allPersons.Length.Should().Be(1);

            personDto.Equals(filter).Should().BeTrue();

        }

        [Fact]
        public void Save_Durable_WhenCalled_Saves_Clears_after_2_seconds()
        {
            //Arrange
            var application = ApplicationHelper.GetWebApplication();
            using var services = application.Services.CreateScope();
            var cacheSvc = services.ServiceProvider.GetRequiredService<ICacheService>();

            int cache_lifetime_secs = 2;

            var personDto = new outPersonDTO
            {
                last_name = "last_name1",
                first_name = "first_name1"
            };
            var filter = BaseService.ModelToDict(personDto);

            IInMemoryProvider sut = new InMemoryProviderImpl(cacheSvc);

            //Act
            sut.Save(personDto, cache_lifetime_secs);

            //Assert
            var allPersons = sut.Fetch(filter);
            allPersons.Should().NotBeNullOrEmpty();

            Task.Delay(cache_lifetime_secs * 1001).Wait();

            allPersons = sut.Fetch(filter);
            allPersons.Should().BeNullOrEmpty();

        }

        [Fact]
        public void Fetch_Durable_WhenCalled_Cross_Connection()
        {
            //Arrange
            var application = ApplicationHelper.GetWebApplicationForCache();
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

            IInMemoryProvider sut1 = new InMemoryProviderImpl(cacheSvc1);
            IInMemoryProvider sut2 = new InMemoryProviderImpl(cacheSvc2);

            //Act
            sut1.Save(personDto);

            //Assert
            var allPersons = sut1.Fetch(filter);
            allPersons.Should().NotBeNullOrEmpty();

            allPersons = sut2.Fetch(filter);
            allPersons.Should().NotBeNullOrEmpty();

        }
    }
}
