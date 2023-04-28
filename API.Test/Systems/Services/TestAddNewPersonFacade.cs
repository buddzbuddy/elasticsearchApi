﻿using elasticsearchApi.Contracts.Infrastructure;
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
using Newtonsoft.Json;

namespace elasticsearchApi.Tests.Systems.Services
{
    [Collection("Sequential")]
    public class TestAddNewPersonFacade : TestUtils
    {
        public TestAddNewPersonFacade(ITestOutputHelper output) : base(output)
        {
        }

        [Fact]
        public void AddNewPerson_WhenCalled_Returns_Empty_OK()
        {
            //Arrange
            var application = ApplicationHelper.GetWebApplication();
            using var services = application.Services.CreateScope();
            var addressRefsVerifier = services.ServiceProvider.GetRequiredService<IAddressRefsVerifier>();
            var databaseMaxCalculator = services.ServiceProvider.GetRequiredService<DatabaseMaxCalculatorProviderImpl>();
            var pinCalculator = services.ServiceProvider.GetRequiredService<IPinCalculator>();

            int regionNo = 3, districtNo = 27;
            //int regCode = regionNo * 1000 + districtNo;


            var addPerson = new addNewPersonDTO
            {
                last_name = $"last_name",
                first_name = $"first_name",
                middle_name = $"middle_name",
                sex = Genders.MALE.GetValueId(),
                date_of_birth = new DateTime(2000, 1, 1),
                passporttype = PassportTypes.PASSPORT.GetValueId(),
                passportseries = "A",
                passportno = $"111222333444555666",
                issuing_authority = $"issuing_authority",
                date_of_issue = DateTime.Now,
                familystate = FamilyStates.MARRIED.GetValueId(),
            };

            var sut = services.ServiceProvider.GetRequiredService<IAddNewPersonFacade>();

            var queryFactory = services.ServiceProvider.GetRequiredService<QueryFactory>();
            var appTransaction = services.ServiceProvider.GetRequiredService<AppTransaction>();

            appTransaction.DisableCommitRollback();
            queryFactory.Connection.Open();
            appTransaction.Transaction = queryFactory.Connection.BeginTransaction();


            /*var pinGenerator = services.ServiceProvider.GetRequiredService<IPinGenerator>();
            var newPin = pinGenerator.GenerateNewPin(in regCode);
            _output.WriteLine(newPin);*/
            try
            {
                //Act
                var context = sut.AddNewPerson(addPerson, in regionNo, in districtNo);

                //var personIdFromDb = queryFactory.Query("Persons").Where("IIN", newPin).Select("id").First<int>(appTransaction.Transaction);

                //Assert
                _output.WriteLine(JsonConvert.SerializeObject(context));
                context.SuccessFlag.Should().BeTrue();
                var newPerson = context["Result"] as outPersonDTO;
                newPerson.Should().NotBeNull();
                var isNewObj = context["IsNew"];
                isNewObj.Should().NotBeNull();
                var isNew = isNewObj as bool?;
                isNew?.Should().BeTrue();

            }
            finally
            {
                appTransaction.Transaction?.Rollback();
                queryFactory.Connection.Close();
            }
        }

        [Fact]
        public void AddNewPerson_WhenCalled_Returns_Data_From_Memory()
        {
            //Arrange
            var application = ApplicationHelper.GetWebApplication();
            using var services = application.Services.CreateScope();
            var addressRefsVerifier = services.ServiceProvider.GetRequiredService<IAddressRefsVerifier>();
            var databaseMaxCalculator = services.ServiceProvider.GetRequiredService<DatabaseMaxCalculatorProviderImpl>();
            var pinCalculator = services.ServiceProvider.GetRequiredService<IPinCalculator>();

            int regionNo = 3, districtNo = 27;
            //int regCode = regionNo * 1000 + districtNo;


            var addPerson = new addNewPersonDTO
            {
                last_name = $"last_name",
                first_name = $"first_name",
                middle_name = $"middle_name",
                sex = Genders.MALE.GetValueId(),
                date_of_birth = new DateTime(2000, 1, 1),
                passporttype = PassportTypes.PASSPORT.GetValueId(),
                passportseries = "A",
                passportno = $"111222333444555666",
                issuing_authority = $"issuing_authority",
                date_of_issue = DateTime.Now,
                familystate = FamilyStates.MARRIED.GetValueId(),
            };

            var sut = services.ServiceProvider.GetRequiredService<IAddNewPersonFacade>();

            var queryFactory = services.ServiceProvider.GetRequiredService<QueryFactory>();
            var appTransaction = services.ServiceProvider.GetRequiredService<AppTransaction>();

            appTransaction.DisableCommitRollback();
            queryFactory.Connection.Open();
            appTransaction.Transaction = queryFactory.Connection.BeginTransaction();


            /*var pinGenerator = services.ServiceProvider.GetRequiredService<IPinGenerator>();
            var newPin = pinGenerator.GenerateNewPin(in regCode);
            _output.WriteLine(newPin);*/
            try
            {
                //Act
                //Make double call to prevent duplication
                var context = sut.AddNewPerson(addPerson, in regionNo, in districtNo);
                context = sut.AddNewPerson(addPerson, in regionNo, in districtNo);

                //var personIdFromDb = queryFactory.Query("Persons").Where("IIN", newPin).Select("id").First<int>(appTransaction.Transaction);

                //Assert
                _output.WriteLine(JsonConvert.SerializeObject(context));
                context.SuccessFlag.Should().BeTrue();
                var newPerson = context["Result"] as outPersonDTO;
                newPerson.Should().NotBeNull();
                var isNewObj = context["IsNew"];
                isNewObj.Should().NotBeNull();
                var isNew = isNewObj as bool?;
                isNew?.Should().BeFalse();
            }
            finally
            {
                appTransaction.Transaction?.Rollback();
                queryFactory.Connection.Close();
            }
        }
    }
}
