using API.Test.Fixtures;
using elasticsearchApi.Controllers;
using elasticsearchApi.Models;
using elasticsearchApi.Services;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace API.Test.Systems.Controllers
{
    public class TestUsersController
    {
        [Fact]
        public async Task Get_OnSuccess_ReturnsStatusCode200()
        {
            //Arrange
            var mockUserService = new Mock<IUserService>();
            mockUserService.Setup(svc => svc.GetAllMyUsers()).ReturnsAsync(MyUserFixtures.GetTestUsers);
            var sut = new UsersController(mockUserService.Object);


            //Act
            var result = (OkObjectResult)await sut.Get();

            //Assert
            result.StatusCode.Should().Be(200);

        }

        [Fact]
        public async Task Get_InvokesUserServiceExactlyOnce()
        {
            //Arrange
            var mockUserService = new Mock<IUserService>();

            mockUserService.Setup(svc => svc.GetAllMyUsers()).ReturnsAsync(MyUserFixtures.GetTestUsers);

            var sut = new UsersController(mockUserService.Object);



            //Act
            var result = await sut.Get();

            //Assert
            mockUserService.Verify(svc => svc.GetAllMyUsers(), Times.Once);

        }

        [Fact]
        public async Task Get_OnSuccess_ReturnsListOfUsers()
        {
            //Arrange
            var mockUserService = new Mock<IUserService>();

            mockUserService.Setup(svc => svc.GetAllMyUsers()).ReturnsAsync(MyUserFixtures.GetTestUsers);

            var sut = new UsersController(mockUserService.Object);



            //Act
            var result = await sut.Get();



            //Assert
            result.Should().BeOfType<OkObjectResult>();
            var ob = (OkObjectResult)result;
            ob.Value.Should().BeOfType<List<MyUserDTO>>();
        }

        [Fact]
        public async Task Get_OnSuccess_ReturnsNotFound()
        {
            //Arrange
            var mockUserService = new Mock<IUserService>();

            mockUserService.Setup(svc => svc.GetAllMyUsers()).ReturnsAsync(new List<elasticsearchApi.Models.MyUserDTO>());

            var sut = new UsersController(mockUserService.Object);



            //Act
            var result = await sut.Get();



            //Assert
            result.Should().BeOfType<NotFoundResult>();

        }
    }
}
