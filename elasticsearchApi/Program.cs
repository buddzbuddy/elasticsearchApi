﻿
using elasticsearchApi.Config;
using elasticsearchApi;
using elasticsearchApi.Data;
using elasticsearchApi.Models;
using elasticsearchApi.Services;
using elasticsearchApi.Utils;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);
var services = builder.Services;
var Configuration = builder.Configuration;
services.AddControllers(options => {
    options.Filters.Add(new AuthorizeFilter());
});
var nrsz_connection = Environment.GetEnvironmentVariable("NRSZ_CONNECTION_STRING");
if (nrsz_connection?.IsNullOrEmpty() ?? false)
{
    nrsz_connection = Configuration["SqlKataSettings:connectionString"];
}
services.AddDbContext<ApiContext>(x => {
    
    x.UseSqlServer(nrsz_connection);
});

AppSettings appSettings = Configuration.GetSection("AppSettings").Get<AppSettings>();
services.AddSingleton(appSettings);
services.AddSwaggerGen();
services.AddAutoMapper(typeof(Program));

services.AddElasticsearch(Configuration);


services.AddSqlKataQueryFactory(Configuration);

services.AddScoped<IElasticService, ElasticService>();
services.AddScoped<IDataService, DataService>();
services.AddTransient<IServiceContext, ServiceContext>();
services.AddTransient<IUserService, UserService>();
services.AddHttpClient<IUserService, UserService>();

services.Configure<UsersApiOptions>(Configuration.GetSection("UsersApiOptions"));

services.AddHostedService<InitiatorHostedService>();

/*builder.Services.AddScoped<IUsers, Users>();
builder.Services.AddSingleton<INotificationService, DummyNotificationService>();*/

services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer();

services.AddAuthorization(config =>
{
    config.DefaultPolicy = new AuthorizationPolicyBuilder()
                                .RequireAuthenticatedUser()
                                .Build();
});

var app = builder.Build();

//app.UseHttpsRedirection();
if (builder.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}
app.UseAuthorization();

app.MapControllers();

app.Run("http://0.0.0.0:5000");

public partial class Program
{

}
