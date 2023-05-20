using elasticsearchApi.Config;
using elasticsearchApi.Data;
using elasticsearchApi.Services;
using elasticsearchApi.Utils;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.EntityFrameworkCore;
using elasticsearchApi.Contracts;
using elasticsearchApi.Utils.InitiatorProcesses;
using elasticsearchApi.Contracts.Passport;
using elasticsearchApi.Services.Passport;
using elasticsearchApi.Models.Infrastructure;
using elasticsearchApi.Contracts.CheckProviders;
using elasticsearchApi.Services.CheckExisting;
using elasticsearchApi.Services.Infrastructure;
using elasticsearchApi.Contracts.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

var services = builder.Services;
var Configuration = builder.Configuration;

var env = builder.Environment;

var nrsz_connection = Environment.GetEnvironmentVariable("NRSZ_CONNECTION_STRING");
if (nrsz_connection.IsNullOrEmpty())
{
    var s = Configuration.GetSection("SqlKataSettings").GetValue<string>("connectionString");
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
services.AddAppTransaction();

services.AddScoped<IElasticService, ElasticServiceImpl>();
services.AddScoped<IDataService, DataServiceImpl>();
services.AddScoped<IServiceContext, ServiceContext>();
services.AddScoped<IUserService, UserServiceImpl>();
services.AddHttpClient<IUserService, UserServiceImpl>();

services.AddModifyPassportServices();

services.AddScoped<ICheckService, CheckServiceImpl>();
services.AddScoped<ICheckFacade, CheckFacadeImpl>();

services.AddPinServices();

services.AddPersonServices();

services.AddCacheServices();

services.AddPassportVerifierServices();
services.AddResolverServices();

services.Configure<UsersApiOptions>(Configuration.GetSection("UsersApiOptions"));
services.AddHostedService<InitiatorHostedService>();

services.AddScoped<IUsers, Users>();
services.AddSingleton<INotificationService, DummyNotificationService>();
services.AddControllers();

var app = builder.Build();

//app.UseHttpsRedirection();
if (builder.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}

app.MapControllers();

app.UseHttpsRedirection();
app.UseSwagger();
app.UseSwaggerUI(options =>
{
    options.SwaggerEndpoint("/swagger/v1/swagger.json", "v1");
    options.RoutePrefix = string.Empty;
});

app.UseRouting();

app.Run();

public partial class Program
{

}
