using elasticsearchApi.Contracts.Person;
using elasticsearchApi.Models.Exceptions.Person;
using elasticsearchApi.Models.Person;
using elasticsearchApi.Services.Person;
using elasticsearchApi.Tests.Infrastructure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit.Abstractions;

namespace elasticsearchApi.Tests.Systems.Services
{
    public class TestAddNewPersonVerifier : TestUtils
    {
        public TestAddNewPersonVerifier(ITestOutputHelper output) : base(output)
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

        }
    }
}
