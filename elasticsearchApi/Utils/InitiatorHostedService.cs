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
            var services = scope.ServiceProvider;
            var _db = services.GetRequiredService<QueryFactory>();
            //Initialize Region and District Codes from DB to InMemory
            var list = await _db.Query("address").Select("regionNo", "districtNo").GetAsync<Refs.RegionDistrictItem>(cancellationToken: cancellationToken);
            Refs.RegionDistricts.AddRange(list);

            Console.WriteLine($"IN-MEMORY DATA({Refs.RegionDistricts.Count} rows) LOADED SUCCESSFUL!");
        }
        public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
    }
}
