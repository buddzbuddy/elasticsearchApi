using Bazinga.AspNetCore.Authentication.Basic;
using elasticsearchApi.Contracts;
using elasticsearchApi.Data;
using elasticsearchApi.Data.Entities;
using elasticsearchApi.Utils.InitiatorProcesses;
using FakeItEasy;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json.Linq;
using System;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Security.Claims;
using System.Threading.Tasks;
using Xunit;

namespace elasticsearchApi.Tests.Repetition
{
    public class UsersControllerTests
    {
        private const string Username = "Test";
        private const string Password = "test";
        private readonly string base64EncodedAuthenticationString = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes($"{Username}:{Password}"));
        //private const string SqlConnectionString = "Server=localhost,14331;Database=nrsz-test;User Id=sa;Password=P@ssword123;Encrypt=False";
        private INotificationService NotificationServiceFake = A.Fake<INotificationService>();

        [Fact]
        public async Task Get_returns_401_Unauthorized_if_not_authenticated()
        {
            var application = GetWebApplication();

            var client = application.CreateClient();

            var response = await client.GetAsync("/users");

            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [Fact]
        public async Task Get_returns_all_users()
        {
            var application = GetWebApplication();

            using (var services = application.Services.CreateScope())
            {
                IDbContextTransaction? transaction = null;
                try
                {
                    var ctx = services.ServiceProvider.GetRequiredService<ApiContext>();
                    transaction = ctx.Database.BeginTransaction();

                    var conn = ctx.Database.GetDbConnection();
                    using (var cmd = conn.CreateCommand())
                    {
                        cmd.Transaction = transaction.GetDbTransaction();
                        cmd.CommandText = "SET IDENTITY_INSERT Users ON; " +
                                        "INSERT INTO Users (Id, FirstName, LastName) VALUES" +
                                        "(1, 'John', 'Doe'), " +
                                        "(2, 'Jane', 'Doe'); " +
                                        "SET IDENTITY_INSERT Users OFF;";
                        await cmd.ExecuteNonQueryAsync();
                    }

                    var client = application.CreateClient();
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", base64EncodedAuthenticationString);

                    var response = await client.GetAsync("/users");
                    var json = await response.Content.ReadAsStringAsync();
                    dynamic users = JArray.Parse(json);

                    Assert.Equal(2, users.Count);
                    Assert.Equal("John", (string)users[0].firstName);
                    Assert.Equal("Doe", (string)users[1].lastName);
                }
                finally
                {
                    transaction?.Rollback();
                }
            }
        }

        [Fact]
        public async Task Put_returns_Created_if_successful()
        {
            var application = GetWebApplication();

            using var services = application.Services.CreateScope();
            IDbContextTransaction? transaction = null;
            try
            {
                var ctx = services.ServiceProvider.GetRequiredService<ApiContext>();
                transaction = ctx.Database.BeginTransaction();

                var client = application.CreateClient();
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", base64EncodedAuthenticationString);

                var response = await client.PutAsJsonAsync("/users/", new { firstName = "John", lastName = "Doe" });

                dynamic user = JObject.Parse(await response.Content.ReadAsStringAsync());

                Assert.Equal("John", (string)user.firstName);
                Assert.Equal("Doe", (string)user.lastName);
                Assert.Equal(System.Net.HttpStatusCode.Created, response.StatusCode);
                Assert.Matches("^http:\\/\\/localhost\\/users\\/\\d+$", response.Headers.Location!.AbsoluteUri.ToLower());

                var userId = int.Parse(response.Headers.Location!.PathAndQuery[(response.Headers.Location!.PathAndQuery.LastIndexOf("/") + 1)..]);

                var conn = ctx.Database.GetDbConnection();
                using var cmd = conn.CreateCommand();
                cmd.Transaction = transaction.GetDbTransaction();
                cmd.CommandText = $"SELECT TOP 1 * FROM Users WHERE Id = {userId}";
                using var rs = await cmd.ExecuteReaderAsync();
                Assert.True(await rs.ReadAsync());
                Assert.Equal("John", rs["FirstName"]);
                Assert.Equal("Doe", rs["LastName"]);
            }
            finally
            {
                transaction?.Rollback();
            }
        }

        [Fact]
        public async Task Put_returns_sends_notification_if_successful()
        {
            var application = GetWebApplication();

            using var services = application.Services.CreateScope();
            IDbContextTransaction? transaction = null;
            try
            {
                var ctx = services.ServiceProvider.GetRequiredService<ApiContext>();
                transaction = ctx.Database.BeginTransaction();

                var client = application.CreateClient();
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", base64EncodedAuthenticationString);

                var response = await client.PutAsJsonAsync("/users/", new { firstName = "John", lastName = "Doe" });

                A.CallTo(() =>
                    NotificationServiceFake.SendUserCreatedNotification(A<User>.That.Matches(x => x.FirstName == "John" && x.LastName == "Doe"))
                ).MustHaveHappened();
            }
            finally
            {
                transaction?.Rollback();
            }
        }

        private WebApplicationFactory<Program> GetWebApplication()
            => new WebApplicationFactory<Program>().WithWebHostBuilder(builder =>
            {
                builder.UseEnvironment("Test");
                builder.ConfigureTestServices(services =>
                {
                    /*var options = new DbContextOptionsBuilder<ApiContext>()
                                    .UseSqlServer(SqlConnectionString)
                                    .Options;
                    services.AddSingleton(options);*/
                    services.AddSingleton<ApiContext>();
                    services.AddSingleton(NotificationServiceFake);

                    services.AddAuthentication()
                            .AddBasicAuthentication(credentials => Task.FromResult(credentials.username == Username && credentials.password == Password));

                    services.AddAuthorization(config =>
                    {
                        config.DefaultPolicy = new AuthorizationPolicyBuilder(config.DefaultPolicy)
                                                    .AddAuthenticationSchemes(BasicAuthenticationDefaults.AuthenticationScheme)
                                                    .Build();
                    });

                    //services.AddHostedService<SeedAddressData>(); //Moved to Program.cs with ENV.TEST condition
                });
            });
    }
}