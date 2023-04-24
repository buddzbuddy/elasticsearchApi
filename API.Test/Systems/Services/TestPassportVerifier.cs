using elasticsearchApi.Contracts.Passport;
using elasticsearchApi.Models;
using elasticsearchApi.Services.Passport;
using elasticsearchApi.Tests.Helpers;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using SqlKata.Execution;
using Xunit.Abstractions;
using System.Data;
using elasticsearchApi.Models.Passport;
using elasticsearchApi.Services.Exceptions.Passport;

namespace elasticsearchApi.Tests.Systems.Services
{
    public class TestPassportVerifier
    {
        private readonly ITestOutputHelper _output;
        public TestPassportVerifier(ITestOutputHelper output)
        {
            _output = output;
        }

        [Fact]
        public void BasicVerifyPassport_WhenInvoked_Throws_PassportException()
        {
            //Arrange
            var mockLogicVerifier = new Mock<IPassportVerifierLogic>();
            var mockDbVerifier = new Mock<IPassportDbVerifier>();
            modifyPersonPassportDTO passport = new modifyPersonPassportDTO
            {
                issuing_authority = "ШВКДн.Ч.Расулов",
                date_of_issue = DateTime.Now,
                familystate = new Guid("A77C7DB9-C27F-4FFC-BFC0-0C6959731B98"),
                passportno = "01660840",
                //passportseries = "А",
                passporttype = new Guid("A77C7DB9-C27F-4FFC-BFC0-0C6959731B98")
            };
            IPassportVerifier sut = new PassportVerifierImpl(new PassportVerifierBasicImpl(), mockLogicVerifier.Object, mockDbVerifier.Object);

            //Act & Assert
            var ex = Assert.Throws<PersonInputErrorException>(() => sut.VerifyPassport(passport));
            ex.Message.Should().Contain("passportseries");
        }

        [Fact]
        public void BasicVerifyPassport_WhenInvoked_Returns_Empty()
        {
            //Arrange
            var mockLogicVerifier = new Mock<IPassportVerifierLogic>();
            var mockDbVerifier = new Mock<IPassportDbVerifier>();
            modifyPersonPassportDTO passport = new modifyPersonPassportDTO
            {
                issuing_authority = "ШВКДн.Ч.Расулов",
                date_of_issue = DateTime.Now,
                familystate = new Guid("A77C7DB9-C27F-4FFC-BFC0-0C6959731B98"),
                passportno = "01660840",
                passportseries = "А",
                passporttype = new Guid("A77C7DB9-C27F-4FFC-BFC0-0C6959731B98")
            };
            IPassportVerifier sut = new PassportVerifierImpl(new PassportVerifierBasicImpl(),
                mockLogicVerifier.Object, mockDbVerifier.Object);

            //Act & Assert
            sut.VerifyPassport(passport);
        }

        [Fact]
        public void LogicVerifyPassport_WhenInvoked_Throws_PassportException()
        {
            //Arrange
            var mockBasicVerifier = new Mock<IPassportVerifier>();
            var mockDbVerifier = new Mock<IPassportDbVerifier>();
            modifyPersonPassportDTO passport1 = new modifyPersonPassportDTO
            {
                familystate = new Guid("A77C7DB9-C27F-4FFC-BFC0-0C6959731B98"),
                passporttype = new Guid("A77C7DB9-C27F-4FFC-BFC0-0C6959731B98")
            };
            modifyPersonPassportDTO passport2 = new modifyPersonPassportDTO
            {
                familystate = new Guid("{6831E05F-9E2F-46DE-AED0-2DE69A8F87D3}"),
                passporttype = new Guid()
            };
            IPassportVerifier sut = new PassportVerifierImpl(mockBasicVerifier.Object,
                new PassportVerifierLogicImpl(), mockDbVerifier.Object);

            //Act & Assert
            var ex = Assert.Throws<PersonInputErrorException>(() => sut.VerifyPassport(passport1));
            ex.Message.Should().Contain("familystate");
            
            ex = Assert.Throws<PersonInputErrorException>(() => sut.VerifyPassport(passport2));
            ex.Message.Should().Contain("passporttype");
        }

        [Fact]
        public void LogicVerifyPassport_WhenInvoked_Returns_Empty()
        {
            //Arrange
            var mockBasicVerifier = new Mock<IPassportVerifier>();
            var mockDbVerifier = new Mock<IPassportDbVerifier>();
            modifyPersonPassportDTO passport = new modifyPersonPassportDTO
            {
                familystate = new Guid("{6831E05F-9E2F-46DE-AED0-2DE69A8F87D3}"),
                passporttype = new Guid("A77C7DB9-C27F-4FFC-BFC0-0C6959731B98")
            };
            IPassportVerifier sut = new PassportVerifierImpl(mockBasicVerifier.Object,
                new PassportVerifierLogicImpl(), mockDbVerifier.Object);

            //Act & Assert
            sut.VerifyPassport(passport);
        }
    }
}
