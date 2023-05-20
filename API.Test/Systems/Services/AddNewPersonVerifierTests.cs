using elasticsearchApi.Contracts.Passport;
using elasticsearchApi.Contracts.Person;
using elasticsearchApi.Models.Contracts;
using elasticsearchApi.Models.Exceptions.Base;
using elasticsearchApi.Models.Exceptions.Passport;
using elasticsearchApi.Models.Exceptions.Person;
using elasticsearchApi.Models.Person;
using elasticsearchApi.Services.Passport;
using elasticsearchApi.Services.Person;
using elasticsearchApi.Tests.Infrastructure;
using FluentAssertions;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit.Abstractions;

namespace elasticsearchApi.Tests.Systems.Services
{
    public class AddNewPersonVerifierTests : TestUtils
    {
        public AddNewPersonVerifierTests(ITestOutputHelper output) : base(output)
        {
        }

        [Fact]
        public void BasicVerifier_Verify_WhenCalled_Throws_Exception()
        {
            //Arrange
            var errorModel = new addNewPersonDTO
            {
                
            };
            IPersonBasicVerifier sut = new PersonBasicVerifierImpl();

            //Act & Assert
            var ex = Assert.Throws<PersonInputErrorException>(() => sut.Verify(errorModel));
            ex.Message.Should().Contain("Last_Name");
            _output.WriteLine(((IReadException)ex).ExceptionType);

        }

        [Fact]
        public void BasicVerifier_Verify_WhenCalled_Returns_OK_Empty()
        {
            //Arrange
            var correctModel = new addNewPersonDTO
            {
                last_name = "last_name",
                first_name = "first_name",
                date_of_birth = DateTime.Now,
                sex = Guid.Empty
            };
            IPersonBasicVerifier sut = new PersonBasicVerifierImpl();

            //Act & Assert
            sut.Verify(correctModel);
        }

        [Fact]
        public void LogicVerifier_Verify_WhenCalled_Throws_Exception()
        {
            //Arrange
            var errorModel = new addNewPersonDTO
            {
                last_name = "last_name1"
            };
            IPersonLogicVerifier sut = new PersonLogicVerifierImpl();

            //Act & Assert
            var ex = Assert.Throws<PersonInputErrorException>(() => sut.Verify(errorModel));
            ex.Message.Should().Contain("Last_Name");
            _output.WriteLine(((IReadException)ex).ExceptionType);
        }

        [Fact]
        public void LogicVerifier_Verify_WhenCalled_Returns_OK_Empty()
        {
            //Arrange
            var correctModel = new addNewPersonDTO
            {
                last_name = "last_name",
                first_name = "first_name",
                date_of_birth = new DateTime(2000,1,1),
                sex = Guid.Parse("74C6C7FE-53C6-4492-A62F-65A7A49AB644")
            };
            IPersonLogicVerifier sut = new PersonLogicVerifierImpl();

            //Act & Assert
            sut.Verify(correctModel);
        }

        [Fact]
        public void VerifyPerson_WhenCalled_Throws_Exception()
        {
            //Arrange
            var errorLogicModel = new addNewPersonDTO
            {
                last_name = "last_name1"
            };
            var okLogicModel = new addNewPersonDTO
            {
                last_name = "last_name",
                first_name = "first_name",
                date_of_birth = new DateTime(2000, 1, 1),
                sex = Guid.Parse("74C6C7FE-53C6-4492-A62F-65A7A49AB644")
            };
            var errorPassport = okLogicModel;
            //errorPassport
            IPersonBasicVerifier personBasicVerifier = new PersonBasicVerifierImpl();
            IPersonLogicVerifier personLogicVerifier = new PersonLogicVerifierImpl();
            var mockPassportVerifier = new Mock<IPassportVerifier>();
            IAddNewPersonVerifier sut = new AddNewPersonVerifierImpl(personBasicVerifier, personLogicVerifier, mockPassportVerifier.Object);


            mockPassportVerifier = new Mock<IPassportVerifier>();
            mockPassportVerifier.Setup(svc => svc.VerifyPassport(It.IsAny<IPassportData>())).Throws<PassportInputErrorException>();
            IAddNewPersonVerifier sutWithPassport = new AddNewPersonVerifierImpl(personBasicVerifier, personLogicVerifier, mockPassportVerifier.Object);

            //Act & Assert
            var ex1 = Assert.Throws<PersonInputErrorException>(() => sut.VerifyPerson(errorLogicModel));
            sut.VerifyPerson(okLogicModel);

            var ex2 = Assert.Throws<PassportInputErrorException>(() => sutWithPassport.VerifyPerson(errorPassport));

            mockPassportVerifier.Verify(svc => svc.VerifyPassport(It.IsAny<IPassportData>()), Times.Once);
        }

        [Fact]
        public void VerifyPerson_WhenCalled_Returns_OK_Empty()
        {
            //Arrange
            var okLogicModel = new addNewPersonDTO
            {
                last_name = "last_name",
                first_name = "first_name",
                date_of_birth = new DateTime(2000, 1, 1),
                sex = Guid.Parse("74C6C7FE-53C6-4492-A62F-65A7A49AB644")
            };
            IPersonBasicVerifier personBasicVerifier = new PersonBasicVerifierImpl();
            IPersonLogicVerifier personLogicVerifier = new PersonLogicVerifierImpl();
            var mockPassportVerifier = new Mock<IPassportVerifier>();
            IAddNewPersonVerifier sut = new AddNewPersonVerifierImpl(personBasicVerifier, personLogicVerifier, mockPassportVerifier.Object);

            //Act & Assert
            sut.VerifyPerson(okLogicModel);
        }
    }
}
