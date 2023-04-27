using elasticsearchApi.Contracts.Infrastructure;
using elasticsearchApi.Contracts.Person;
using elasticsearchApi.Contracts.PinGenerator;
using elasticsearchApi.Models.Infrastructure;
using elasticsearchApi.Models.Person;
using elasticsearchApi.Models;
using elasticsearchApi.Services.PinGenerator.MaxCalculatorProviders;
using elasticsearchApi.Tests.Helpers;
using elasticsearchApi.Tests.Infrastructure;
using FluentAssertions;
using SqlKata.Execution;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit.Abstractions;
using Microsoft.Extensions.DependencyInjection;
using elasticsearchApi.Utils;
using elasticsearchApi.Contracts.DataProviders;
using elasticsearchApi.Services.DataProviders;
using elasticsearchApi.Services;

namespace elasticsearchApi.Tests.Systems.Services
{
    [Collection("Sequential")]
    public class TestCreatePersonFacade : TestUtils
    {
        public TestCreatePersonFacade(ITestOutputHelper output) : base(output)
        {
        }

        [Fact]
        public void CreateNewPerson_WhenCalled_Returns_NewPerson()
        {
            //Arrange
            var application = ApplicationHelper.GetWebApplication();
            using var services = application.Services.CreateScope();
            var addressRefsVerifier = services.ServiceProvider.GetRequiredService<IAddressRefsVerifier>();
            var databaseMaxCalculator = services.ServiceProvider.GetRequiredService<DatabaseMaxCalculatorProviderImpl>();
            var pinCalculator = services.ServiceProvider.GetRequiredService<IPinCalculator>();

            int regionNo = 3, districtNo = 27;
            int regCode = regionNo * 1000 + districtNo;

            
            var addPerson = new addNewPersonDTO
            {
                last_name = $"last_name-{DateTime.Now:yyyy-MM-dd HH:mm}",
                first_name = $"first_name-{DateTime.Now:yyyy-MM-dd HH:mm}",
                middle_name = $"middle_name-{DateTime.Now:yyyy-MM-dd HH:mm}",
                sex = Genders.MALE.GetValueId(),
                date_of_birth = new DateTime(2000, 1, 1),
                passporttype = PassportTypes.PASSPORT.GetValueId(),
                passportseries = "A",
                passportno = $"{DateTime.Now:yyyy-MM-dd HH:mm}",
                issuing_authority = $"issuing_authority-{DateTime.Now:yyyy-MM-dd HH:mm}",
                date_of_issue = DateTime.Now,
                familystate = FamilyStates.MARRIED.GetValueId(),
            };

            var sut = services.ServiceProvider.GetRequiredService<ICreatePersonFacade>();

            var queryFactory = services.ServiceProvider.GetRequiredService<QueryFactory>();
            var appTransaction = services.ServiceProvider.GetRequiredService<AppTransaction>();

            appTransaction.DisableCommitRollback();
            queryFactory.Connection.Open();
            appTransaction.Transaction = queryFactory.Connection.BeginTransaction();


            var pinGenerator = services.ServiceProvider.GetRequiredService<IPinGenerator>();
            var newPin = pinGenerator.GenerateNewPin(in regCode);
            _output.WriteLine(newPin);
            try
            {
                //Act
                var newPerson = sut.CreateNewPerson(addPerson, in regionNo, in districtNo);

                var personIdFromDb = queryFactory.Query("Persons").Where("IIN", newPin).Select("id").First<int>(appTransaction.Transaction);

                //Assert
                newPin.Length.Should().Be(14);
                personIdFromDb.Should().Be(newPerson.id);
            }
            finally
            {
                appTransaction.Transaction?.Rollback();
                queryFactory.Connection.Close();
            }
        }

        [Fact]
        public void CreateNewPerson_WhenCalled_Compares_With_Memory()
        {
            //Arrange
            var application = ApplicationHelper.GetWebApplication();
            using var services = application.Services.CreateScope();
            var addressRefsVerifier = services.ServiceProvider.GetRequiredService<IAddressRefsVerifier>();
            var databaseMaxCalculator = services.ServiceProvider.GetRequiredService<DatabaseMaxCalculatorProviderImpl>();
            var pinCalculator = services.ServiceProvider.GetRequiredService<IPinCalculator>();

            int regionNo = 3, districtNo = 27;
            int regCode = regionNo * 1000 + districtNo;


            var addPerson = new addNewPersonDTO
            {
                last_name = $"last_name-{DateTime.Now:yyyy-MM-dd HH:mm}",
                first_name = $"first_name-{DateTime.Now:yyyy-MM-dd HH:mm}",
                middle_name = $"middle_name-{DateTime.Now:yyyy-MM-dd HH:mm}",
                sex = Genders.MALE.GetValueId(),
                date_of_birth = new DateTime(2000, 1, 1),
                passporttype = PassportTypes.PASSPORT.GetValueId(),
                passportseries = "A",
                passportno = $"{DateTime.Now:yyyy-MM-dd HH:mm}",
                issuing_authority = $"issuing_authority-{DateTime.Now:yyyy-MM-dd HH:mm}",
                date_of_issue = DateTime.Now,
                familystate = FamilyStates.MARRIED.GetValueId(),
            };

            var sut = services.ServiceProvider.GetRequiredService<ICreatePersonFacade>();

            var queryFactory = services.ServiceProvider.GetRequiredService<QueryFactory>();
            var appTransaction = services.ServiceProvider.GetRequiredService<AppTransaction>();


            IInMemoryProvider inMemoryProvider = services.ServiceProvider.GetRequiredService<IInMemoryProvider>();


            appTransaction.DisableCommitRollback();
            queryFactory.Connection.Open();
            appTransaction.Transaction = queryFactory.Connection.BeginTransaction();


            var pinGenerator = services.ServiceProvider.GetRequiredService<IPinGenerator>();
            var newPin = pinGenerator.GenerateNewPin(in regCode);
            _output.WriteLine(newPin);
            try
            {
                //Act
                var newPerson = sut.CreateNewPerson(addPerson, in regionNo, in districtNo);

                var personIdFromDb = queryFactory.Query("Persons").Where("IIN", newPin).Select("id").First<int>(appTransaction.Transaction);

                //Assert
                newPin.Length.Should().Be(14);
                personIdFromDb.Should().Be(newPerson.id);

                var filter = BaseService.ModelToDict(newPerson);

                var memPersons = inMemoryProvider.Fetch(filter);
                memPersons.Should().NotBeNull();
                memPersons?.Length.Should().Be(1);
                memPersons?[0].id.Should().Be(newPerson.id);
                _output.WriteLine(memPersons?[0].iin);
            }
            finally
            {
                appTransaction.Transaction?.Rollback();
                queryFactory.Connection.Close();
            }
        }
    }
}
