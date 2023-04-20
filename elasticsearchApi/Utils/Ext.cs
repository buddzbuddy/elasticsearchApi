using elasticsearchApi.Contracts;
using elasticsearchApi.Contracts.Passport;
using elasticsearchApi.Models;
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
using System.Reflection.Emit;

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

    public static class AppTransactionExtensions
    {
        public static void AddAppTransaction(this IServiceCollection services) => services.AddScoped((e) => new AppTransaction());
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

    public static class ObjectExtensions
    {
        static ObjectExtensions()
        {
            var assemblyBuilder = AssemblyBuilder.DefineDynamicAssembly(new AssemblyName("Web"), AssemblyBuilderAccess.RunAndCollect);
            ModuleBuilder = assemblyBuilder.DefineDynamicModule("MainModule");
        }

        public static object Extend(this object obj, params object[] anotherObject)
        {
            var properties = anotherObject
                .Concat(new[] { obj })
                .SelectMany(o => o.GetType().GetProperties(), (o, p) => (o, p))
                .ToDictionary(t => t.p.Name, t => (propName: t.p.Name, propType: t.p.PropertyType, propValue: t.p.GetValue(t.o)));

            var objType = CreateClass(properties);

            var finalObj = Activator.CreateInstance(objType);
            foreach (var prop in objType.GetProperties())
                prop.SetValue(finalObj, properties[prop.Name].propValue);

            return finalObj;
        }

        const MethodAttributes METHOD_ATTRIBUTES = MethodAttributes.Public | MethodAttributes.SpecialName | MethodAttributes.HideBySig;
        private static ModuleBuilder ModuleBuilder;

        internal static Type CreateClass(IDictionary<string, (string propName, Type type, object)> parameters)
        {
            var typeBuilder = ModuleBuilder.DefineType(Guid.NewGuid().ToString(), TypeAttributes.Public | TypeAttributes.Class | TypeAttributes.AutoClass | TypeAttributes.AnsiClass | TypeAttributes.BeforeFieldInit | TypeAttributes.AutoLayout, null);
            typeBuilder.DefineDefaultConstructor(MethodAttributes.Public | MethodAttributes.SpecialName | MethodAttributes.RTSpecialName);
            foreach (var parameter in parameters)
                CreateProperty(typeBuilder, parameter.Key, parameter.Value.type);
            var type = typeBuilder.CreateTypeInfo().AsType();
            return type;
        }

        private static PropertyBuilder CreateProperty(TypeBuilder typeBuilder, string propertyName, Type propertyType)
        {
            var fieldBuilder = typeBuilder.DefineField("_" + propertyName, propertyType, FieldAttributes.Private);

            var propBuilder = typeBuilder.DefineProperty(propertyName, PropertyAttributes.HasDefault, propertyType, null);
            propBuilder.SetGetMethod(DefineGet(typeBuilder, fieldBuilder, propBuilder));
            propBuilder.SetSetMethod(DefineSet(typeBuilder, fieldBuilder, propBuilder));

            return propBuilder;
        }

        private static MethodBuilder DefineSet(TypeBuilder typeBuilder, FieldBuilder fieldBuilder, PropertyBuilder propBuilder)
            => DefineMethod(typeBuilder, $"set_{propBuilder.Name}", null, new[] { propBuilder.PropertyType }, il =>
            {
                il.Emit(OpCodes.Ldarg_0);
                il.Emit(OpCodes.Ldarg_1);
                il.Emit(OpCodes.Stfld, fieldBuilder);
                il.Emit(OpCodes.Ret);
            });

        private static MethodBuilder DefineGet(TypeBuilder typeBuilder, FieldBuilder fieldBuilder, PropertyBuilder propBuilder)
            => DefineMethod(typeBuilder, $"get_{propBuilder.Name}", propBuilder.PropertyType, Type.EmptyTypes, il =>
            {
                il.Emit(OpCodes.Ldarg_0);
                il.Emit(OpCodes.Ldfld, fieldBuilder);
                il.Emit(OpCodes.Ret);
            });

        private static MethodBuilder DefineMethod(TypeBuilder typeBuilder, string methodName, Type propertyType, Type[] parameterTypes, Action<ILGenerator> bodyWriter)
        {
            var methodBuilder = typeBuilder.DefineMethod(methodName, METHOD_ATTRIBUTES, propertyType, parameterTypes);
            bodyWriter(methodBuilder.GetILGenerator());
            return methodBuilder;
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