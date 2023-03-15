using elasticsearchApi.Models;
using elasticsearchApi.Services;
using elasticsearchApi.Utils;
using FluentAssertions;
using Moq;
using SqlKata.Execution;

namespace elasticsearchApi.Tests.Systems.Services
{
    public class TestDataService
    {
        [Fact]
        public void ModifyPersonPassport_WhenCalled_ReturnsINPUT_DATA_ERROR()
        {
            //Arrange
            /*var nrsz_connection = "Data Source=192.168.0.68;Initial Catalog=nrsz;Password=WestWood-911;Persist Security Info=True;User ID=sa;MultipleActiveResultSets=True;Max Pool Size=3500;Connect Timeout=300;";
            SqlConnection connection = new(nrsz_connection);
            SqlServerCompiler compiler = new();*/
            var mockServiceContext = new Mock<IServiceContext>();
            var mockEsService = new Mock<IElasticService>();
            var appSettings = new AppSettings();
            var mockQF = new Mock<QueryFactory>();
            string iin = "";
            //mockQF.Setup(_db => _db.Query("Persons").Where("IIN", iin).FirstOrDefault(null, null)).Returns(null);
            //mockServiceContext.Setup(svc => svc.AddErrorMessage(It.IsAny<string>(), It.IsAny<string>()));
            mockServiceContext.Setup(svc => svc.ErrorMessages.Count).Returns(1);

            var sut = new DataService(mockQF.Object, appSettings, mockEsService.Object);

            //Act
            var sc = mockServiceContext.Object;
            var response = sut.ModifyPersonPassport(iin, new modifyPersonPassportDTO(), ref sc);

            //Assert

            response.Should().Be(ModifyPersonPassportResult.INPUT_DATA_ERROR);
        }
        [Fact]
        public void ModifyPersonPassport_WhenCalled_Returns_NRSZ_NOT_FOUND_BY_PIN()
        {
            //Arrange
            /*var nrsz_connection = "Data Source=192.168.0.68;Initial Catalog=nrsz;Password=WestWood-911;Persist Security Info=True;User ID=sa;MultipleActiveResultSets=True;Max Pool Size=3500;Connect Timeout=300;";
            SqlConnection connection = new(nrsz_connection);
            SqlServerCompiler compiler = new();*/
            var mockServiceContext = new Mock<IServiceContext>();
            var mockEsService = new Mock<IElasticService>();
            var appSettings = new AppSettings();
            var mockQF = new Mock<QueryFactory>();
            string iin = "";
            mockQF.Setup(_db => _db.Get(_db.Query("Persons").Where("IIN", iin), null, null)).Returns(Array.Empty<dynamic>().AsEnumerable());
            //mockServiceContext.Setup(svc => svc.AddErrorMessage(It.IsAny<string>(), It.IsAny<string>()));
            mockServiceContext.Setup(svc => svc.ErrorMessages.Count).Returns(0);

            var sut = new DataService(mockQF.Object, appSettings, mockEsService.Object);

            //Act
            var sc = mockServiceContext.Object;
            var response = sut.ModifyPersonPassport(iin, new modifyPersonPassportDTO(), ref sc);

            //Assert

            response.Should().Be(ModifyPersonPassportResult.NRSZ_NOT_FOUND_BY_PIN);
        }
    }
}
