using elasticsearchApi.Contracts.Passport;
using elasticsearchApi.Models;
using elasticsearchApi.Services.Exceptions;
using elasticsearchApi.Services.Passport;
using elasticsearchApi.Tests.Helpers;
using elasticsearchApi.Tests.Infrastructure;
using elasticsearchApi.Utils;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using SqlKata.Execution;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit.Abstractions;

namespace elasticsearchApi.Tests.Systems.Services
{
    public class TestModifyPassportDataService : TestUtils
    {
        public TestModifyPassportDataService(ITestOutputHelper output) : base(output)
        {
        }

        [Fact]
        public void Execute_WhenCalled_Throws_PersonNotFoundException()
        {
            //Arrange
            string iin = "123";
            var mockPassportVerifier = new Mock<IPassportVerifier>();
            var application = ApplicationHelper.GetWebApplication();
            using var services = application.Services.CreateScope();
            var _db = services.ServiceProvider.GetRequiredService<QueryFactory>();
            IModifyPassportDataService sut = new ModifyPassportDataServiceImpl(_db, mockPassportVerifier.Object);
            //Act & Assert
            var ex = Assert.Throws<PersonNotFoundException>(() => sut.Execute(iin, new modifyPersonPassportDTO(), null));
        }

        [Fact]
        public void Execute_WhenCalled_Throws_PassportDuplicateException()
        {
            //Arrange
            Guid passporttype = new Guid("{A77C7DB9-C27F-4FFC-BFC0-0C6959731B98}");//Passport
            string iin1 = "1111111111111", iin2 = "1111111111112", passportseries = "A";
            var personObj1 = new modifyPersonPassportDTO
            {
                passporttype = passporttype,
                passportseries = passportseries,
                passportno = "666666"
            };
            var personObj2 = new modifyPersonPassportDTO
            {
                passporttype = passporttype,
                passportseries = passportseries,
                passportno = "666667"
            };
            var mockPassportVerifier = new Mock<IPassportVerifier>();
            var application = ApplicationHelper.GetWebApplication();
            using var services = application.Services.CreateScope();
            var _db = services.ServiceProvider.GetRequiredService<QueryFactory>();
            IDbTransaction? transaction = null;
            IModifyPassportDataService sut = new ModifyPassportDataServiceImpl(_db, mockPassportVerifier.Object);
            try
            {
                _db.Connection.Open();
                transaction = _db.Connection.BeginTransaction();
                int affects1 = _db.Query("Persons").Insert(personObj1.Extend(new { iin = iin1 }), transaction);
                int affects2 = _db.Query("Persons").Insert(personObj2.Extend(new { iin = iin2 }), transaction);

                //Act & Assert
                affects1.Should().Be(1);
                affects2.Should().Be(1);
                var ex = Assert.Throws<PassportDuplicateException>(() => sut.Execute(iin2, personObj1, transaction));
            }
            finally
            {
                transaction?.Rollback();
            }
        }
    }
}
