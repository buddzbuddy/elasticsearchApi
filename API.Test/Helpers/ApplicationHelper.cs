using Bazinga.AspNetCore.Authentication.Basic;
using elasticsearchApi.Contracts;
using elasticsearchApi.Data;
using elasticsearchApi.Models.Infrastructure;
using elasticsearchApi.Services;
using elasticsearchApi.Utils;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using SqlKata.Compilers;
using SqlKata.Execution;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace elasticsearchApi.Tests.Helpers
{
    public static class ApplicationHelper
    {
        public static WebApplicationFactory<Program> GetWebApplication()
            => new WebApplicationFactory<Program>().WithWebHostBuilder(builder =>
            {
                builder.UseEnvironment("Test");
                builder.ConfigureTestServices(services =>
                {
                    services.AddSingleton<ApiContext>();
                    services.AddSingleton<AppTransaction>();
                    services.AddSingleton((e) =>
                    {
                        SqlConnection connection = new("Server=192.168.2.150,14331;Database=nrsz-test;User Id=sa;Password=P@ssword123;Encrypt=False");
                        SqlServerCompiler compiler = new();
                        return new QueryFactory(connection, compiler);
                    });
                });
            });
        public static WebApplicationFactory<Program> GetWebApplicationForCache()
            => new WebApplicationFactory<Program>().WithWebHostBuilder(builder =>
            {
                builder.UseEnvironment("Test");
                builder.ConfigureTestServices(services =>
                {
                    services.AddTransient<ICacheService, CacheServiceImpl>();
                    services.AddTransient<ICacheProvider, CacheProviderImpl>();
                });
            });
        public static WebApplicationFactory<Program> GetWebApplicationStandard()
            => new WebApplicationFactory<Program>().WithWebHostBuilder(builder =>
            {
                builder.UseEnvironment("Test");
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
    }
}
