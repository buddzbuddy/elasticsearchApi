using elasticsearchApi.Contracts.Passport;
using elasticsearchApi.Models;
using elasticsearchApi.Models.Exceptions.Base;
using elasticsearchApi.Models.Exceptions.Passport;
using elasticsearchApi.Models.Exceptions.Person;
using elasticsearchApi.Models.Infrastructure;
using elasticsearchApi.Models.Passport;
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
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit.Abstractions;

namespace elasticsearchApi.Tests.Systems.Services
{
    [Collection("Sequential")]
    public class ModifyPassportDataServiceTests : TestUtils
    {
        public ModifyPassportDataServiceTests(ITestOutputHelper output) : base(output)
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
            var appTransaction = services.ServiceProvider.GetRequiredService<AppTransaction>();
            _db.Connection.Open();
            var existingPassportVerifier = services.ServiceProvider.GetRequiredService<IExistingPassportVerifier>();
            IModifyPassportDataService sut = new ModifyPassportDataServiceImpl(_db, mockPassportVerifier.Object, appTransaction, existingPassportVerifier);
            //Act & Assert
            var ex = Assert.Throws<PersonNotFoundException>(() => sut.Execute(iin, new modifyPersonPassportDTO()));
            _db.Connection.Close();
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
                passportno = "66666601"
            };
            var personObj2 = new modifyPersonPassportDTO
            {
                passporttype = passporttype,
                passportseries = passportseries,
                passportno = "66666701"
            };
            var mockPassportVerifier = new Mock<IPassportVerifier>();
            var application = ApplicationHelper.GetWebApplication();
            using var services = application.Services.CreateScope();
            var _db = services.ServiceProvider.GetRequiredService<QueryFactory>();
            var appTransaction = services.ServiceProvider.GetRequiredService<AppTransaction>();
            var existingPassportVerifier = services.ServiceProvider.GetRequiredService<IExistingPassportVerifier>();
            IModifyPassportDataService sut = new ModifyPassportDataServiceImpl(_db, mockPassportVerifier.Object, appTransaction, existingPassportVerifier);
            try
            {
                _db.Connection.Open();
                appTransaction.Transaction = _db.Connection.BeginTransaction();
                int affects1 = _db.Query("Persons").Insert(personObj1.Extend(new { iin = iin1 }), appTransaction.Transaction);
                int affects2 = _db.Query("Persons").Insert(personObj2.Extend(new { iin = iin2 }), appTransaction.Transaction);

                //Act & Assert
                affects1.Should().Be(1);
                affects2.Should().Be(1);
                var ex = Assert.Throws<PassportDuplicateException>(() => sut.Execute(iin2, personObj1));
                _output.WriteLine(ex.Message);
            }
            finally
            {
                appTransaction.Transaction?.Rollback();
                _db.Connection.Close();
            }
        }

        [Fact]
        public void Execute_WhenInvoked_Throws_ReadExceptions()
        {
            //Arrange
            string iinExisting = "20180001252734", iinIncorrect = "1111111111112";
            var incorrectModel = new modifyPersonPassportDTO();
            var duplicateModel = new modifyPersonPassportDTO
            {
                passporttype = PassportTypes.PASSPORT.GetValueId(),
                passportseries = "А",
                passportno = "2356567",
                date_of_issue = DateTime.ParseExact("2009-06-17", "yyyy-MM-dd", CultureInfo.InvariantCulture),
                familystate = FamilyStates.SINGLE.GetValueId(),
                issuing_authority = "САХШ ш.Парчакент"
            };
            var correctModel = new modifyPersonPassportDTO
            {
                passporttype = PassportTypes.PASSPORT.GetValueId(),
                passportseries = "А",
                passportno = "666777",
                date_of_issue = DateTime.ParseExact("2009-06-17", "yyyy-MM-dd", CultureInfo.InvariantCulture),
                familystate = FamilyStates.MARRIED.GetValueId(),
                issuing_authority = "issuing_authority"
            };
            var application = ApplicationHelper.GetWebApplication();
            using var services = application.Services.CreateScope();
            var _db = services.ServiceProvider.GetRequiredService<QueryFactory>();
            var appTransaction = services.ServiceProvider.GetRequiredService<AppTransaction>();
            _db.Connection.Open();

            var passportVerifier = services.ServiceProvider.GetRequiredService<IPassportVerifier>();
            var existingPassportVerifier = services.ServiceProvider.GetRequiredService<IExistingPassportVerifier>();
            IModifyPassportDataService sut = new ModifyPassportDataServiceImpl(_db, passportVerifier, appTransaction, existingPassportVerifier);
            //Act & Assert
            var ex1 = Assert.ThrowsAny<Exception>(() => sut.Execute(iinIncorrect, incorrectModel));
            var ex2 = Assert.ThrowsAny<Exception>(() => sut.Execute(iinIncorrect, correctModel));
            var ex3 = Assert.ThrowsAny<Exception>(() => sut.Execute(iinExisting, duplicateModel));

            Assert.True(ex1 is IReadException and PassportInputErrorException);
            Assert.True(ex2 is IReadException and PersonNotFoundException);
            Assert.True(ex3 is IReadException and PassportDuplicateException);

            _db.Connection.Close();
            _output.WriteLine(((IReadException)ex3).ExceptionType);
        }

        [Fact(Skip = "Couldn't come up with event that will trigger these types of exceptions")]
        public void Execute_WhenInvoked_Throws_WriteExceptions()
        {
            //Arrange
            Guid
                documentType1 = new Guid("{A77C7DB9-C27F-4FFC-BFC0-0C6959731B98}"),//Passport
                documentType2 = new Guid("{A52BE3AF-5DFA-405B-A4E6-18A64C24F9A5}");//BirthCertificate
            Guid
                familystate1 = new Guid("{6831E05F-9E2F-46DE-AED0-2DE69A8F87D3}"),//Married
                familystate2 = new Guid("{3C58D432-147C-491A-A34A-4A88B2CCBCB5}"),//Single
                familystate3 = new Guid("{2900D318-9207-4241-9CD0-A0B6D6DBC75F}"),//Widower/Widow
                familystate4 = new Guid("{EF783D94-0418-4ABA-B653-6DB2A10E4B92}");//Divorced
            string iinExisting = "20180001252734", iinIncorrect = "1111111111112";
            var incorrectModel = new modifyPersonPassportDTO();
            var duplicateModel = new modifyPersonPassportDTO
            {
                passporttype = documentType1,
                passportseries = "А",
                passportno = "2356567",
                date_of_issue = DateTime.ParseExact("2009-06-17", "yyyy-MM-dd", CultureInfo.InvariantCulture),
                familystate = familystate2,
                issuing_authority = "САХШ ш.Парчакент"
            };
            var correctModel = new modifyPersonPassportDTO
            {
                passporttype = documentType1,
                passportseries = "А",
                passportno = "666777",
                date_of_issue = DateTime.ParseExact("2009-06-17", "yyyy-MM-dd", CultureInfo.InvariantCulture),
                familystate = familystate1,
                issuing_authority = "issuing_authority"
            };
            var application = ApplicationHelper.GetWebApplication();
            using var services = application.Services.CreateScope();
            var _db = services.ServiceProvider.GetRequiredService<QueryFactory>();
            var appTransaction = services.ServiceProvider.GetRequiredService<AppTransaction>();

            IPassportVerifierBasic passportVerifierBasic = new PassportVerifierBasicImpl();
            IPassportVerifierLogic passportVerifierLogic = new PassportVerifierLogicImpl();
            IPassportVerifier passportVerifier = new PassportVerifierImpl(passportVerifierBasic, passportVerifierLogic);
            var mockExistingPassportVerifier = new Mock<IExistingPassportVerifier>();
            IModifyPassportDataService sut = new ModifyPassportDataServiceImpl(_db, passportVerifier, appTransaction, mockExistingPassportVerifier.Object);

            //Act & Assert
            var ex1 = Assert.ThrowsAny<Exception>(() => sut.Execute(iinIncorrect, incorrectModel));
            var ex2 = Assert.ThrowsAny<Exception>(() => sut.Execute(iinIncorrect, correctModel));

            Assert.True(ex1 is IWriteException and PersonInputErrorException);
            Assert.True(ex2 is IWriteException and PersonNotFoundException);


            _output.WriteLine(((IReadException)ex2).ExceptionType);
        }



        [Fact]
        public void Execute_WhenInvoked_Returns_OK_Empty()
        {
            //Arrange
            Guid
                documentType1 = new Guid("{A77C7DB9-C27F-4FFC-BFC0-0C6959731B98}"),//Passport
                documentType2 = new Guid("{A52BE3AF-5DFA-405B-A4E6-18A64C24F9A5}");//BirthCertificate
            Guid
                familystate1 = new Guid("{6831E05F-9E2F-46DE-AED0-2DE69A8F87D3}"),//Married
                familystate2 = new Guid("{3C58D432-147C-491A-A34A-4A88B2CCBCB5}"),//Single
                familystate3 = new Guid("{2900D318-9207-4241-9CD0-A0B6D6DBC75F}"),//Widower/Widow
                familystate4 = new Guid("{EF783D94-0418-4ABA-B653-6DB2A10E4B92}");//Divorced
            string iinExisting = "30470000214";
            var correctModel = new modifyPersonPassportDTO
            {
                passporttype = documentType1,
                passportseries = "А",
                passportno = "6667773",
                date_of_issue = DateTime.ParseExact("2009-06-17", "yyyy-MM-dd", CultureInfo.InvariantCulture),
                familystate = familystate1,
                issuing_authority = "issuing_authority"
            };
            var application = ApplicationHelper.GetWebApplication();
            using var services = application.Services.CreateScope();
            var _db = services.ServiceProvider.GetRequiredService<QueryFactory>();
            var appTransaction = services.ServiceProvider.GetRequiredService<AppTransaction>();
            _db.Connection.Open();
            var prevPerson = _db.Query("Persons").Where("IIN", iinExisting).FirstOrDefault();
            var prevPassportCount = _db.Query("Passports").Where("PersonId", (int)prevPerson.Id).Count<int>();

            IPassportVerifierBasic passportVerifierBasic = new PassportVerifierBasicImpl();
            IPassportVerifierLogic passportVerifierLogic = new PassportVerifierLogicImpl();
            IPassportVerifier passportVerifier = new PassportVerifierImpl(passportVerifierBasic, passportVerifierLogic);
            var existingPassportVerifier = services.ServiceProvider.GetRequiredService<IExistingPassportVerifier>();
            IModifyPassportDataService sut = new ModifyPassportDataServiceImpl(_db, passportVerifier, appTransaction, existingPassportVerifier);
            IDbTransaction? transaction = null;

            //Act & Assert
            try
            {
                sut.Execute(iinExisting, correctModel);

                var result = _db.Query("Persons").Where(UtilHelper.ConvertToDictionary(correctModel)).Count<int>(transaction: appTransaction.Transaction);
                var newPassportCount = _db.Query("Passports").Where("PersonId", (int)prevPerson.Id).Count<int>(transaction: appTransaction.Transaction);

                result.Should().Be(1);
                (newPassportCount - prevPassportCount).Should().Be(1);
            }
            finally
            {
                transaction?.Rollback();
                _db.Connection.Close();
            }
        }
    }
}
