using elasticsearchApi.Models;
using elasticsearchApi.Utils;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
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
            services.Configure<Utils.AppSettings>(Configuration.GetSection("AppSettings"));
            services.AddSwaggerGen();
            services.AddAutoMapper(typeof(Startup));
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
            app.UseHttpsRedirection();
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
            FakeDb.RegCounters = new Dictionary<int, int>();
            // Create a new scope to retrieve scoped services
            using var scope = _serviceProvider.CreateScope();
            //3. Get the instance of BoardGamesDBContext in our services layer
            var services = scope.ServiceProvider;
            var _appSettings = services.GetRequiredService<IOptions<AppSettings>>();
            //Initialize PIN counters from DB
            using var connection = new SqlConnection(_appSettings.Value.cissa_data_connection);
            connection.Open();
            var command = new SqlCommand(getRegCounterSql, connection);
            using (var reader = await command.ExecuteReaderAsync(cancellationToken))
            {
                while (reader.Read()) FakeDb.RegCounters.Add(reader.GetInt32(0), reader.GetInt32(1));
            }
            command = new(getDistrictsSql, connection);
            using (var reader = await command.ExecuteReaderAsync(cancellationToken))
            {
                while (reader.Read()) Refs.RegionDistricts.Add(new Refs.RegionDistrictItem { RegionNo = reader.GetInt32(0), DistrictNo = reader.GetInt32(1) });
            }
            connection.Close();

            Console.WriteLine("STARTUP CODE EXECUTED SUCCESSFUL");
        }
        const string getRegCounterSql = @"
SELECT CAST(SUBSTRING([iin], 0, 5) as int) as regCode, CAST(MAX(SUBSTRING([iin], 5, 10)) as int) as maxPin
FROM [nrsz-data].[dbo].[nrsz_persons]
where iin like '[0-9][0-9][0-9][0-9][0-9][0-9][0-9][0-9][0-9][0-9][0-9][0-9][0-9][0-9]'
group by SUBSTRING([iin], 0, 5)
";
        const string getDistrictsSql = @"
SELECT
	[Area1].[Number] as regionNo,
	[District].[Number] as [districtNo]
FROM
	(SELECT
		d.Id,
		[a1].[Value] as [Number],
		[a2].[Value] as [Area]
	FROM
		Documents d WITH(NOLOCK)
		INNER JOIN Int_Attributes a1 WITH(NOLOCK) on (a1.Document_Id = d.Id and a1.Def_Id = '8f74f065-4632-4ef4-87e3-ae0df1d3cfca' and a1.Expired = '99991231' and [a1].[Value] > 0)
		LEFT OUTER JOIN Document_Attributes a2 WITH(NOLOCK) on (a2.Document_Id = d.Id and a2.Def_Id = 'e6600d4b-f03c-4cb9-a67a-8b400cdf6f69' and a2.Expired = '99991231')
	WHERE
		d.Def_Id = 'ba5d4276-6bfb-4180-9d4f-828e38e95601' AND
		([d].[Deleted] is null OR [d].[Deleted] = 0)
	) as [District]
	INNER JOIN (SELECT
		d.Id,
		[a1].[Value] as [Number]
	FROM
		Documents d WITH(NOLOCK)
		INNER JOIN Int_Attributes a1 WITH(NOLOCK) on (a1.Document_Id = d.Id and a1.Def_Id = '9f8a9e68-676f-4c1d-9359-0784df076491' and a1.Expired = '99991231' and [a1].[Value] > 0)
	WHERE
		d.Def_Id = '67a81660-8a8b-4d80-9199-734a618edd32'
	) as [Area] on [Area].Id = [District].[Area]
	INNER JOIN (SELECT
		d.Id,
		[a1].[Value] as [Number]
	FROM
		Documents d WITH(NOLOCK)
		LEFT OUTER JOIN Int_Attributes a1 WITH(NOLOCK) on (a1.Document_Id = d.Id and a1.Def_Id = '9f8a9e68-676f-4c1d-9359-0784df076491' and a1.Expired = '99991231')
	WHERE
		d.Def_Id = '67a81660-8a8b-4d80-9199-734a618edd32'
	) as [Area1] on [Area1].Id = [District].[Area]
";
        public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
    }
}
