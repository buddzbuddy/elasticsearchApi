using elasticsearchApi.Models;
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
            using var connection = new SqlConnection(Environment.GetEnvironmentVariable("ASIST_DATA_CONNECTION_STRING"));
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
SELECT
	[Area].[Number] as regionNo,
	[District].[Number] as [districtNo]
FROM
	(SELECT
		d.Id,
		[a1].[Value] as [Number],
		[a2].[Value] as [Area]
	FROM
		Documents d WITH(NOLOCK)
		INNER JOIN Int_Attributes a1 WITH(NOLOCK) on (a1.Document_Id = d.Id and a1.Def_Id = '{B31C64A2-0606-471D-BED2-CBBBD5E9CE79}' and a1.Expired = '99991231' and [a1].[Value] > 0)
		INNER JOIN Document_Attributes a2 WITH(NOLOCK) on (a2.Document_Id = d.Id and a2.Def_Id = '{485F6AEF-4ACC-4240-BD79-93C940E6F6C4}' and a2.Expired = '99991231')
	WHERE
		d.Def_Id = '{4D029337-C025-442E-8E93-AFD1852073AC}' AND
		([d].[Deleted] is null OR [d].[Deleted] = 0)
	) as [District]
	INNER JOIN (SELECT
		d.Id,
		[a1].[Value] as [Number]
	FROM
		Documents d WITH(NOLOCK)
		INNER JOIN Int_Attributes a1 WITH(NOLOCK) on (a1.Document_Id = d.Id and a1.Def_Id = '{F63E743F-557C-4DB3-A599-69A06C134C6C}' and a1.Expired = '99991231' and [a1].[Value] > 0)
	WHERE
		d.Def_Id = '{8C5E9217-59AC-4B4E-A41A-643FC34444E4}'
	) as [Area] on [Area].Id = [District].[Area]
";
        public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
    }
}
