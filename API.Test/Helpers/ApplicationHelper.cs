using Bazinga.AspNetCore.Authentication.Basic;
using elasticsearchApi.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using SqlKata.Execution;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace elasticsearchApi.Tests.Helpers
{
    public static class ApplicationHelper
    {
        private const string Username = "Test";
        private const string Password = "test";
        public static WebApplicationFactory<Program> GetWebApplication()
            => new WebApplicationFactory<Program>().WithWebHostBuilder(builder =>
            {
                builder.UseEnvironment("Test");
                builder.ConfigureTestServices(services =>
                {
                    services.AddScoped<ApiContext>();

                    /*services.AddAuthentication()
                            .AddBasicAuthentication(credentials => Task.FromResult(credentials.username == Username && credentials.password == Password));

                    services.AddAuthorization(config =>
                    {
                        config.DefaultPolicy = new AuthorizationPolicyBuilder(config.DefaultPolicy)
                                                    .AddAuthenticationSchemes(BasicAuthenticationDefaults.AuthenticationScheme)
                                                    .Build();
                    });*/
                });
            });
    }
}
