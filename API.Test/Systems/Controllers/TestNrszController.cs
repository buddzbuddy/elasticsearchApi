using elasticsearchApi.Contracts.Passport;
using elasticsearchApi.Models;
using elasticsearchApi.Services.Passport;
using elasticsearchApi.Tests.Helpers;
using elasticsearchApi.Tests.Infrastructure;
using elasticsearchApi.Utils;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
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
    [Collection("Sequential")]
    public class TestNrszController : TestUtils
    {
        public TestNrszController(ITestOutputHelper output) : base(output)
        {
        }

        [Fact]
        public async Task ModifyPassport_WhenCalled_Returns_Ok_Empty()
        {
            //Arrange
            var application = ApplicationHelper.GetWebApplication();
            using var services = application.Services.CreateScope();
            var _db = services.ServiceProvider.GetRequiredService<QueryFactory>();
            var appTransaction = services.ServiceProvider.GetRequiredService<AppTransaction>();

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
                passportno = "6667772",
                date_of_issue = DateTime.ParseExact("2009-06-17", "yyyy-MM-dd", CultureInfo.InvariantCulture),
                familystate = familystate1,
                issuing_authority = "issuing_authority"
            };


            var prevPerson = _db.Query("Persons").Where("IIN", iinExisting).FirstOrDefault();
            var prevPassportCount = _db.Query("Passports").Where("PersonId", (int)prevPerson.Id).Count<int>();

            var url = $"api/NrszPersons/ModifyPersonPassport?iin={iinExisting}";

            var client = application.CreateHttpClientJson();
            //Act & Assert
            try
            {
                _db.Connection.Open();
                appTransaction.Transaction = _db.Connection.BeginTransaction();

                appTransaction.DisableCommitRollback();

                var response = await client.PostAsync(url, ApplicationHelper.CreateBodyContent(correctModel));
                response.EnsureSuccessStatusCode();
                var json = await response.Content.ReadAsStringAsync();
                var resContext = JObject.Parse(json).ToObject<ServiceContext>();


                resContext.Should().NotBeNull();
                if (!resContext?.SuccessFlag ?? false) _output.WriteLine(resContext?.ErrorMessages.ToReadableString());
                resContext?.SuccessFlag.Should().BeTrue();
                
                var result = _db.Query("Persons").Where(UtilHelper.ConvertToDictionary(correctModel)).Count<int>(transaction: appTransaction.Transaction);
                var newPassportCount = _db.Query("Passports").Where("PersonId", (int)prevPerson.Id).Count<int>(transaction: appTransaction.Transaction);

                result.Should().Be(1);
                (newPassportCount - prevPassportCount).Should().Be(1);
            }
            finally
            {
                appTransaction.Transaction?.Rollback();
                _db.Connection.Close();
            }
        }

        [Fact]
        public async Task ModifyPassport_WhenCalled_Returns_404_ErrorMessages()
        {
            //Arrange
            var application = ApplicationHelper.GetWebApplication();
            using var services = application.Services.CreateScope();
            var _db = services.ServiceProvider.GetRequiredService<QueryFactory>();
            var appTransaction = services.ServiceProvider.GetRequiredService<AppTransaction>();

            Guid
                documentType1 = new Guid("{A77C7DB9-C27F-4FFC-BFC0-0C6959731B98}"),//Passport
                documentType2 = new Guid("{A52BE3AF-5DFA-405B-A4E6-18A64C24F9A5}");//BirthCertificate
            Guid
                familystate1 = new Guid("{6831E05F-9E2F-46DE-AED0-2DE69A8F87D3}"),//Married
                familystate2 = new Guid("{3C58D432-147C-491A-A34A-4A88B2CCBCB5}"),//Single
                familystate3 = new Guid("{2900D318-9207-4241-9CD0-A0B6D6DBC75F}"),//Widower/Widow
                familystate4 = new Guid("{EF783D94-0418-4ABA-B653-6DB2A10E4B92}");//Divorced
            string iinExisting = "506800022201";
            var correctModel = new modifyPersonPassportDTO
            {
                passporttype = documentType1,
                passportseries = "А",
                passportno = "666777",
                date_of_issue = DateTime.ParseExact("2009-06-17", "yyyy-MM-dd", CultureInfo.InvariantCulture),
                familystate = familystate1,
                issuing_authority = "issuing_authority"
            };

            /*
            var prevPerson = _db.Query("Persons").Where("IIN", iinExisting).FirstOrDefault();
            var prevPassportCount = _db.Query("Passports").Where("PersonId", (int)prevPerson.Id).Count<int>();
            */
            var url = $"api/NrszPersons/ModifyPersonPassport?iin={iinExisting}";

            var client = application.CreateHttpClientJson();
            //Act & Assert
            try
            {
                _db.Connection.Open();
                appTransaction.Transaction = _db.Connection.BeginTransaction();

                appTransaction.DisableCommitRollback();

                var response = await client.PostAsync(url, ApplicationHelper.CreateBodyContent(correctModel));
                response.EnsureSuccessStatusCode();
                var json = await response.Content.ReadAsStringAsync();
                var resContext = JObject.Parse(json).ToObject<ServiceContext>();


                resContext.Should().NotBeNull();
                if (!(resContext?.SuccessFlag ?? false))
                {
                    _output.WriteLine(json);
                    _output.WriteLine(resContext?.ErrorMessages.ToReadableString());
                }
                resContext?.SuccessFlag.Should().BeFalse();
            }
            finally
            {
                appTransaction.Transaction?.Rollback();
                _db.Connection.Close();
            }
        }

        [Fact]
        public async Task ModifyPassport_WhenCalled_Returns_PassportDuplicate_ErrorMessages()
        {
            //Arrange
            var application = ApplicationHelper.GetWebApplication();
            using var services = application.Services.CreateScope();
            var _db = services.ServiceProvider.GetRequiredService<QueryFactory>();
            var appTransaction = services.ServiceProvider.GetRequiredService<AppTransaction>();

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
                passportno = "9988776",
                date_of_issue = DateTime.ParseExact("2009-06-17", "yyyy-MM-dd", CultureInfo.InvariantCulture),
                familystate = familystate1,
                issuing_authority = "issuing_authority"
            };

            /*
            var prevPerson = _db.Query("Persons").Where("IIN", iinExisting).FirstOrDefault();
            var prevPassportCount = _db.Query("Passports").Where("PersonId", (int)prevPerson.Id).Count<int>();
            */
            var url = $"api/NrszPersons/ModifyPersonPassport?iin={iinExisting}";

            var client = application.CreateHttpClientJson();
            //Act & Assert
            try
            {
                _db.Connection.Open();
                appTransaction.Transaction = _db.Connection.BeginTransaction();

                appTransaction.DisableCommitRollback();

                var response = await client.PostAsync(url, ApplicationHelper.CreateBodyContent(correctModel));
                response.EnsureSuccessStatusCode();
                var json = await response.Content.ReadAsStringAsync();
                var resContext = JObject.Parse(json).ToObject<ServiceContext>();


                resContext.Should().NotBeNull();
                if (!(resContext?.SuccessFlag ?? false))
                {
                    _output.WriteLine(json);
                    _output.WriteLine(resContext?.ErrorMessages.ToReadableString());
                }
                resContext?.SuccessFlag.Should().BeFalse();
            }
            finally
            {
                appTransaction.Transaction?.Rollback();
                _db.Connection.Close();
            }
        }
    }
}
