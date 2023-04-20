using Bazinga.AspNetCore.Authentication.Basic;
using elasticsearchApi.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using SqlKata.Execution;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
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

        public static HttpClient CreateHttpClientJson(this WebApplicationFactory<Program> application)
        {
            var client = application.CreateClient();

            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            return client;
        }
        public static HttpClient CreateHttpClientJson(this WebApplicationFactory<Program> application, string bearerToken)
        {
            var client = application.CreateHttpClientJson();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", bearerToken);
            return client;
        }

        public static HttpContent CreateBodyContent(object body)
        {
            string dataJson = JsonConvert.SerializeObject(body);
            var data = new StringContent(dataJson, Encoding.UTF8, "application/json");
            return data;
        }

        /*public static AuthResponseDto? LoginUser(this WebApplicationFactory<Program> application, string username, string password)
        {
            var loginClient = application.CreateHttpClientJson();
            var loginCredData = CreateBodyContent(new { username, password });

            var loginResultResponse = loginClient.PostAsync("api/Users/Login", loginCredData).Result;
            var loginResultJson = loginResultResponse.Content.ReadAsStringAsync().Result;
            return JsonConvert.DeserializeObject<AuthResponseDto>(loginResultJson);
        }*/
    }
}
