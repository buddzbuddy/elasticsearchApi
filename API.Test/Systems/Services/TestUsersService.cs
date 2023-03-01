using API.Test.Fixtures;
using API.Test.Helpers;
using elasticsearchApi.Config;
using elasticsearchApi.Models;
using elasticsearchApi.Services;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Moq;
using Moq.Protected;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using static System.Net.WebRequestMethods;

namespace API.Test.Systems.Services
{
    public class TestUsersService
    {
        [Fact]
        public async Task GetAllUsers_WhenCalled_InvokesHttpGetRequest()
        {
            //Arrange
            var expectedResponse = MyUserFixtures.GetTestUsers();
            var handlerMock = MockHttpMessageHandler<MyUserDTO>.SetupBasicGetResourceList(expectedResponse);
            var httpClient = new HttpClient(handlerMock.Object);
            var endpoint = "http://example.com/users";
            var config = Options.Create(new UsersApiOptions
            {
                Endpoint = endpoint
            });
            var sut = new UserService(httpClient, config);

            //Act
            await sut.GetAllMyUsers();


            //Assert
            handlerMock.Protected().Verify(
                "SendAsync", Times.Exactly(1), ItExpr.Is<HttpRequestMessage>(req => req.Method == HttpMethod.Get),
                ItExpr.IsAny<CancellationToken>()
                );
        }

        [Fact]
        public async Task GetAllUsers_WhenCalled_ReturnsListOfUsersOfExpectedSize()
        {
            //Arrange
            var expectedResponse = MyUserFixtures.GetTestUsers();
            var handlerMock = MockHttpMessageHandler<MyUserDTO>.SetupBasicGetResourceList(expectedResponse);
            var httpClient = new HttpClient(handlerMock.Object);
            var endpoint = "http://example.com/users";
            var config = Options.Create(new UsersApiOptions
            {
                Endpoint = endpoint
            });
            var sut = new UserService(httpClient, config);

            //Act
            var result = await sut.GetAllMyUsers();


            //Assert
            result.Should().BeOfType<List<MyUserDTO>>();
            result.Count.Should().Be(expectedResponse.Count);
        }

        [Fact]
        public async Task GetAllUsers_WhenHits404_ReturnsEmptyListOfUsers()
        {
            //Arrange
            var endpoint = "http://example.com/users";
            var expectedResponse = MyUserFixtures.GetTestUsers();
            var handlerMock = MockHttpMessageHandler<MyUserDTO>.SetupReturn404(expectedResponse);
            var httpClient = new HttpClient(handlerMock.Object);
            var config = Options.Create(new UsersApiOptions
            {
                Endpoint = endpoint
            });
            var sut = new UserService(httpClient, config);

            //Act
            var result = await sut.GetAllMyUsers();


            //Assert
            result.Count.Should().Be(0);
        }

        [Fact]
        public async Task GetAllUsers_WhenCalled_InvokesConfiguredExternalUrl()
        {
            //Arrange
            var expectedResponse = MyUserFixtures.GetTestUsers();
            //TOOK FROM APPSETTINGS
            var endpoint = "https://example.com/users";
            var handlerMock = MockHttpMessageHandler<MyUserDTO>.SetupBasicGetResourceList(expectedResponse);
            var httpClient = new HttpClient(handlerMock.Object);

            var config = Options.Create(new UsersApiOptions
            {
                Endpoint = endpoint
            });

            var sut = new UserService(httpClient, config);

            //Act
            var result = await sut.GetAllMyUsers();
            var requestUri = new Uri(endpoint);
            var httpRequestMessage = new HttpRequestMessage
            {
                RequestUri = new Uri(endpoint),
                Method = HttpMethod.Get
            };
            //Assert
            handlerMock.Protected().Verify(
                "SendAsync", Times.Exactly(1),
                httpRequestMessage,
                ItExpr.IsAny<CancellationToken>()
                );
        }
    }
}
