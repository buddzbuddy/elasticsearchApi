using elasticsearchApi.Contracts.Passport;
using elasticsearchApi.Models;
using elasticsearchApi.Services.Passport;
using elasticsearchApi.Tests.Helpers;
using elasticsearchApi.Tests.Infrastructure;
using FluentAssertions;
using Newtonsoft.Json.Linq;
using SqlKata.Execution;
using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit.Abstractions;
using static System.Net.Mime.MediaTypeNames;

namespace elasticsearchApi.Tests.Systems.Controllers
{
    public class TestNrszController : TestUtils
    {
        public TestNrszController(ITestOutputHelper output) : base(output)
        {
        }

        public async Task ModifyPassport_WhenCalled_Returns_Errors()
        {
            var url = "api/NrszPersons/ModifyPersonPassport";
            var client = application.CreateHttpClientJson(loginResult?.Token ?? "");
            var response = await client.GetAsync(fetchUrl);
            response.EnsureSuccessStatusCode();
            var json = await response.Content.ReadAsStringAsync();
            dynamic res1 = JObject.Parse(json);
            var list = (JArray)res1.data;
            list.Count.Should().BeGreaterThan(0);
            var instrObj = ((JObject)list[0]).ToObject<financeInstrumentDTO>();
            instrObj.Should().NotBeNull();
            //Arrange
            Guid
                documentType1 = new Guid("{A77C7DB9-C27F-4FFC-BFC0-0C6959731B98}"),//Passport
                documentType2 = new Guid("{A52BE3AF-5DFA-405B-A4E6-18A64C24F9A5}");//BirthCertificate
            Guid
                familystate1 = new Guid("{6831E05F-9E2F-46DE-AED0-2DE69A8F87D3}"),//Married
                familystate2 = new Guid("{3C58D432-147C-491A-A34A-4A88B2CCBCB5}"),//Single
                familystate3 = new Guid("{2900D318-9207-4241-9CD0-A0B6D6DBC75F}"),//Widower/Widow
                familystate4 = new Guid("{EF783D94-0418-4ABA-B653-6DB2A10E4B92}");//Divorced
            string iinExisting = "50680002220";
            var correctModel = new modifyPersonPassportDTO
            {
                passporttype = documentType1,
                passportseries = "А",
                passportno = "6667771",
                date_of_issue = DateTime.ParseExact("2009-06-17", "yyyy-MM-dd", CultureInfo.InvariantCulture),
                familystate = familystate1,
                issuing_authority = "issuing_authority"
            };
            var application = ApplicationHelper.GetWebApplication();
            using var services = application.Services.CreateScope();
            var _db = services.ServiceProvider.GetRequiredService<QueryFactory>();

            var prevPerson = _db.Query("Persons").Where("IIN", iinExisting).FirstOrDefault();
            var prevPassportCount = _db.Query("Passports").Where("PersonId", (int)prevPerson.Id).Count<int>();

            IPassportVerifierBasic passportVerifierBasic = new PassportVerifierBasicImpl();
            IPassportVerifierLogic passportVerifierLogic = new PassportVerifierLogicImpl();
            IPassportDbVerifier passportDbVerifier = new PassportDbVerifierImpl(_db);
            IPassportVerifier passportVerifier = new PassportVerifierImpl(passportVerifierBasic, passportVerifierLogic, passportDbVerifier);
            IModifyPassportDataService dataSvc = new ModifyPassportDataServiceImpl(_db, passportVerifier);
            IModifyPassportActor sut = new ModifyPassportActorImpl(dataSvc);

            _db.Connection.Open();
            IDbTransaction? transaction = _db.Connection.BeginTransaction();

            //Act & Assert
            try
            {
                var context = sut.CallModifyPassport(iinExisting, correctModel, null, null, ref transaction);


                context.SuccessFlag.Should().BeTrue();
                var result = _db.Query("Persons").Where(UtilHelper.ConvertToDictionary(correctModel)).Count<int>(transaction: transaction);
                var newPassportCount = _db.Query("Passports").Where("PersonId", (int)prevPerson.Id).Count<int>(transaction: transaction);

                result.Should().Be(1);
                (newPassportCount - prevPassportCount).Should().Be(1);
            }
            finally
            {
                transaction?.Rollback();
            }
        }
    }
}
