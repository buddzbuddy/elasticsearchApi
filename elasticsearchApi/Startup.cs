using elasticsearchApi.Models;
using elasticsearchApi.Services;
using elasticsearchApi.Utils;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using System;
using System.Data.SqlClient;
using System.Threading;
using System.Threading.Tasks;

namespace elasticsearchApi
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllers();
            AppSettings appSettings = Configuration.GetSection("AppSettings").Get<AppSettings>();
            services.AddSingleton(appSettings);
            services.AddSwaggerGen();
            services.AddAutoMapper(typeof(Startup));

            services.AddElasticsearch(Configuration);


            services.AddSqlKataQueryFactory(Configuration);

            services.AddScoped<IElasticService, ElasticService>();
            services.AddScoped<IDataService, DataService>();
            services.AddTransient<IServiceContext, ServiceContext>();

            services.AddHostedService<InitiatorHostedService>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            app.UseAuthentication();
            app.UseAuthorization();
            //app.UseHttpsRedirection();
            app.UseSwagger();
            app.UseSwaggerUI(options =>
            {
                options.SwaggerEndpoint("/swagger/v1/swagger.json", "v1");
                options.RoutePrefix = string.Empty;
            });

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }

    public class InitiatorHostedService : IHostedService
    {
        // We need to inject the IServiceProvider so we can create 
        // the scoped service, MyDbContext
        private readonly IServiceProvider _serviceProvider;
        public InitiatorHostedService(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            Console.WriteLine("STARTUP SQL LOADING STARTED...");
            // Create a new scope to retrieve scoped services
            using var scope = _serviceProvider.CreateScope();
            //3. Get the instance of BoardGamesDBContext in our services layer
            var services = scope.ServiceProvider;
            var _appSettings = services.GetRequiredService<AppSettings>();
            //Initialize PIN counters from DB
            using var connection = new SqlConnection(Environment.GetEnvironmentVariable("NRSZ_CONNECTION_STRING"));
            connection.Open();
            var command = new SqlCommand(getDistrictsSql, connection);
            using (var reader = await command.ExecuteReaderAsync(cancellationToken))
            {
                while (reader.Read()) Refs.RegionDistricts.Add(new Refs.RegionDistrictItem { RegionNo = reader.GetInt32(0), DistrictNo = reader.GetInt32(1) });
            }
            connection.Close();
            Console.WriteLine($"IN-MEMORY DATA({Refs.RegionDistricts.Count} rows) LOADED SUCCESSFUL!");
        }
        const string getRegCounterSql = @"
SELECT CAST(SUBSTRING([iin], 0, 5) as int) as regCode, CAST(MAX(SUBSTRING([iin], 5, 10)) as int) as maxPin
FROM [nrsz-data].[dbo].[nrsz_persons]
where iin like '[0-9][0-9][0-9][0-9][0-9][0-9][0-9][0-9][0-9][0-9][0-9][0-9][0-9][0-9]'
group by SUBSTRING([iin], 0, 5)
";
        const string getDistrictsSql = @"
SELECT [regionNo]
      ,[districtNo]
  FROM [nrsz].[dbo].[address]
";
        public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
    }
}
