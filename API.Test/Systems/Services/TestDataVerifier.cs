using elasticsearchApi.Services;
using FluentAssertions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace elasticsearchApi.Tests.Systems.Services
{
    public class TestDataVerifier
    {
        [Fact]
        public void Verify_WhenCalled_ReturnsFalse() {
            //Arrange
            var sut = new DataVerifier();

            //Act
            var result = sut.Verify(new Models.modifyPersonPassportDTO(), out Dictionary<string, string> dic);


            //Assert
            result.Should().BeFalse();
        }

        //TODO: Implement all error types of verification individually


        [Fact]
        public void Verify_WhenCalled_ReturnsTrue()
        {
            //Arrange
            var passportTypeId = new Guid("{A77C7DB9-C27F-4FFC-BFC0-0C6959731B98}");
            var obj = new Models.modifyPersonPassportDTO()
            {
                date_of_issue = DateTime.UtcNow,
                familystate = Guid.Empty,
                passporttype = passportTypeId,
                passportno = "123456",
                passportseries = "A",
                issuing_authority = "Some Court"
            };
            var sut = new DataVerifier();

            //Act
            var result = sut.Verify(obj, out Dictionary<string, string> dic);


            //Assert
            result.Should().BeTrue();
        }
    }
}
