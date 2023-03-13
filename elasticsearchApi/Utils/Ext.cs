using Humanizer.Configuration;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Nest;
using SqlKata.Compilers;
using SqlKata.Execution;
using System;
using System.Data.SqlClient;

namespace elasticsearchApi.Utils
{
    public static class MyExtensions
    {
        /// <summary>
        ///     A string extension method that query if '@this' is empty.
        /// </summary>
        /// <param name="this">The @this to act on.</param>
        /// <returns>true if empty, false if not.</returns>
        public static bool IsEmpty(this string @this)
        {
            return string.IsNullOrEmpty(@this);
        }
        public static bool IsNullOrEmpty(this object @this)
        {
            return @this == null || string.IsNullOrEmpty(@this.ToString());
        }

        public static string ToStringJoin(this string[] @this, string separator = ",")
        {
            return @this == null ? "" : string.Join(separator, @this);
        }
    }
    public static class ElasticsearchExtensions
    {
        public static void AddElasticsearch(this IServiceCollection services, IConfiguration configuration)
        {
            //var settings = new ConnectionSettings(new Uri(configuration["ElasticsearchSettings:uri"]));
            var es_host = Environment.GetEnvironmentVariable("ES_HOST");
            if(es_host.IsNullOrEmpty()) es_host = configuration["ElasticsearchSettings:uri"];

            var settings = new ConnectionSettings(new Uri(es_host));
            
            var defaultIndex = configuration["ElasticsearchSettings:defaultIndex"];
            var elasticUser = configuration["ElasticsearchSettings:elasticUser"];
            var elasticPass = configuration["ElasticsearchSettings:elasticPass"];
            settings = settings.BasicAuthentication(elasticUser, elasticPass);
            if (!string.IsNullOrEmpty(defaultIndex))
                settings = settings.DefaultIndex(defaultIndex);
            var client = new ElasticClient(settings);
            services.AddSingleton<IElasticClient>(client);
        }
    }
    public static class SqlKataExtensions
    {
        public static void AddSqlKataQueryFactory(this IServiceCollection services, IConfiguration configuration)
        {
            var nrsz_connection = Environment.GetEnvironmentVariable("NRSZ_CONNECTION_STRING");
            if (nrsz_connection.IsNullOrEmpty())
            {
                nrsz_connection = configuration["SqlKataSettings:connectionString"];
            }
            services.AddTransient((e) =>
            {
                SqlConnection connection = new(nrsz_connection);
                SqlServerCompiler compiler = new();
                return new QueryFactory(connection, compiler);
            });
        }
    }

}