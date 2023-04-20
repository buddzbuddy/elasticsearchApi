using elasticsearchApi.Config;
using elasticsearchApi.Data;
using elasticsearchApi.Models;
using elasticsearchApi.Services;
using elasticsearchApi.Utils;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.EntityFrameworkCore;
using elasticsearchApi.Contracts;
using elasticsearchApi.Utils.InitiatorProcesses;

var builder = WebApplication.CreateBuilder(args);

var services = builder.Services;
var Configuration = builder.Configuration;

var env = builder.Environment;
services.AddControllers(options => {
    options.Filters.Add(new AuthorizeFilter());
});
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

services.AddScoped<IElasticService, ElasticServiceImpl>();
services.AddScoped<IDataService, DataServiceImpl>();
services.AddScoped<IServiceContext, ServiceContext>();
services.AddScoped<IUserService, UserServiceImpl>();
services.AddHttpClient<IUserService, UserServiceImpl>();
services.AddCacheServices();

services.AddPassportVerifierServices();

services.Configure<UsersApiOptions>(Configuration.GetSection("UsersApiOptions"));
services.AddHostedService<InitiatorHostedService>();

services.AddScoped<IUsers, Users>();
services.AddSingleton<INotificationService, DummyNotificationService>();

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

app.Run();

public partial class Program
{

}
