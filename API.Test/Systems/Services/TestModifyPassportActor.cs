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
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit.Abstractions;

namespace elasticsearchApi.Tests.Systems.Services
{
    public class TestModifyPassportActor : TestUtils
    {
        public TestModifyPassportActor(ITestOutputHelper output) : base(output)
        {
        }

        [Fact]
        public void CallModifyPassport_WhenInvoked_Returns_Error_From_ReadExceptions()
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

            IPassportVerifierBasic passportVerifierBasic = new PassportVerifierBasicImpl();
            IPassportVerifierLogic passportVerifierLogic = new PassportVerifierLogicImpl();
            IPassportDbVerifier passportDbVerifier = new PassportDbVerifierImpl(_db);
            IPassportVerifier passportVerifier = new PassportVerifierImpl(passportVerifierBasic, passportVerifierLogic, passportDbVerifier);
            IModifyPassportDataService dataSvc = new ModifyPassportDataServiceImpl(_db, passportVerifier);
            IModifyPassportActor sut = new ModifyPassportActorImpl(dataSvc);

            //Act & Assert
            var context1 = sut.CallModifyPassport(iinIncorrect, incorrectModel);
            var context2 = sut.CallModifyPassport(iinIncorrect, correctModel);
            var context3 = sut.CallModifyPassport(iinExisting, duplicateModel);

            context1.ErrorMessages["type"].Should().Contain("ModifyPassportActor");
            context2.ErrorMessages["type"].Should().Contain("ModifyPassportActor");
            context3.ErrorMessages["type"].Should().Contain("ModifyPassportActor");
        }
    }
}
