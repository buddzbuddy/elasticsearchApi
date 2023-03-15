using elasticsearchApi.Models;
using SqlKata.Execution;

namespace elasticsearchApi.Utils
{
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
            var _config = services.GetRequiredService<IConfiguration>();
            var _db = services.GetRequiredService<QueryFactory>();
            //Initialize Region and District Codes from DB to InMemory
            var list = await _db.Query("address").Select("regionNo", "districtNo").GetAsync<Refs.RegionDistrictItem>();
            Refs.RegionDistricts.AddRange(list);

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
