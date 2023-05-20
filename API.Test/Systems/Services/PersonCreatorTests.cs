using elasticsearchApi.Contracts.Infrastructure;
using elasticsearchApi.Contracts.Person;
using elasticsearchApi.Contracts.PinGenerator;
using elasticsearchApi.Models;
using elasticsearchApi.Models.Infrastructure;
using elasticsearchApi.Models.Person;
using elasticsearchApi.Services.PinGenerator.MaxCalculatorProviders;
using elasticsearchApi.Tests.Helpers;
using elasticsearchApi.Tests.Infrastructure;
using elasticsearchApi.Utils;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Nest;
using SqlKata.Execution;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit.Abstractions;

namespace elasticsearchApi.Tests.Systems.Services
{
    [Collection("Sequential")]
    public class PersonCreatorTests : TestUtils
    {
        public PersonCreatorTests(ITestOutputHelper output) : base(output)
        {
        }
        [Fact]
        public void CreateNewPerson_WhenCalled_Returns_NewPersonId()
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
            
            var pinGenerator = services.ServiceProvider.GetRequiredService<IPinGenerator>();
            var newPin = pinGenerator.GenerateNewPin(in regCode);
            _output.WriteLine(newPin);

            var newPerson = new addNewPersonDTO
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

            var sut = services.ServiceProvider.GetRequiredService<IPersonCreator>();

            var queryFactory = services.ServiceProvider.GetRequiredService<QueryFactory>();
            var appTransaction = services.ServiceProvider.GetRequiredService<AppTransaction>();

            appTransaction.DisableCommitRollback();
            queryFactory.Connection.Open();
            appTransaction.Transaction = queryFactory.Connection.BeginTransaction();
            try
            {
                //Act
                var newPersonId = sut.CreateNewPerson(newPerson, newPin);

                var personIdFromDb = queryFactory.Query("Persons").Where("IIN", newPin).Select("id").First<int>(appTransaction.Transaction);

                //Assert
                newPin.Length.Should().Be(14);
                personIdFromDb.Should().Be(newPersonId);
            }
            finally
            {
                appTransaction.Transaction?.Rollback();
                queryFactory.Connection.Close();
            }
        }
    }
}
