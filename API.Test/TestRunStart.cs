﻿using elasticsearchApi.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.DependencyInjection;
using Nest;
using Xunit.Abstractions;
using Xunit.Sdk;

[assembly: Xunit.TestFramework("elasticsearchApi.Tests.TestRunStart", "elasticsearchApi.Tests")]
namespace elasticsearchApi.Tests
{
    public class TestRunStart : XunitTestFramework
    {
        public TestRunStart(IMessageSink messageSink) : base(messageSink)
        {
            var options = new DbContextOptionsBuilder<ApiContext>()
                                .UseSqlServer("Server=192.168.2.150,14331;Database=nrsz-test;User Id=sa;Password=P@ssword123;Encrypt=False");
            var dbContext = new ApiContext(options.Options);
            dbContext.Database.EnsureCreated();
            dbContext.Database.Migrate();
        }
    }
}
