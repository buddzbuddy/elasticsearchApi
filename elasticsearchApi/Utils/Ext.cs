using elasticsearchApi.Contracts;
using elasticsearchApi.Contracts.Passport;
using elasticsearchApi.Services;
using elasticsearchApi.Services.Passport;
using Humanizer.Configuration;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Nest;
using SqlKata.Compilers;
using SqlKata.Execution;
using System;
using System.ComponentModel;
using System.Data.Common;
using System.Data.SqlClient;
using System.Reflection;

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
        public static bool IsNullOrEmpty(this object? @this)
        {
            return @this == null || string.IsNullOrEmpty(@this.ToString());
        }

        public static string ToStringJoin(this string[] @this, string separator = ",")
        {
            return @this == null ? "" : string.Join(separator, @this);
        }
        public static string? GetDescription(this Enum value)
        {
            Type type = value.GetType();
            var name = Enum.GetName(type, value);
            if (name != null)
            {
                var field = type.GetField(name);
                if (field != null)
                {
                    if (Attribute.GetCustomAttribute(field,
                             typeof(DescriptionAttribute)) is DescriptionAttribute attr)
                    {
                        return attr.Description;
                    }
                }
            }
            return value?.ToString();
        }
    }
    public static class ElasticsearchExtensions
    {
        public static void AddElasticsearch(this IServiceCollection services, IConfiguration configuration)
        {
            //var settings = new ConnectionSettings(new Uri(configuration["ElasticsearchSettings:uri"]));
            var es_host = Environment.GetEnvironmentVariable("ES_HOST");
            if(es_host.IsNullOrEmpty()) es_host = configuration["ElasticsearchSettings:uri"];

            var settings = new ConnectionSettings(new Uri(es_host ?? String.Empty));
            
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
            services.AddScoped((e) =>
            {
                SqlConnection connection = new(nrsz_connection);
                SqlServerCompiler compiler = new();
                return new QueryFactory(connection, compiler);
            });
        }
    }
    public static class CacheServiceExtensions
    {
        public static void AddCacheServices(this IServiceCollection services)
        {
            services.AddMemoryCache();
            services.AddScoped<ICacheService, CacheServiceImpl>();
            services.AddScoped<ICacheProvider, CacheProviderImpl>();
        }
    }
    public static class DataVerifierExtensions
    {
        public static void AddPassportVerifierServices(this IServiceCollection services)
        {
            services.AddScoped<IPassportVerifier, PassportVerifierImpl>();
            services.AddScoped<IPassportVerifierBasic, PassportVerifierBasicImpl>();
            services.AddScoped<IPassportVerifierLogic, PassportVerifierLogicImpl>();
            services.AddScoped<IPassportDbVerifier, PassportDbVerifierImpl>();
        }
    }

    /*public static class EFExtensions
    {
        public static DbTransaction GetDbTransaction(this IDbContextTransaction source)
        {
            return (source as IInfrastructure<DbTransaction>).Instance;
        }
    }*/


}