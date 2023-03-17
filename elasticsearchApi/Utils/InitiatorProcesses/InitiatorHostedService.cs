using elasticsearchApi.Data.Entities;
using elasticsearchApi.Models;
using elasticsearchApi.Services;
using SqlKata.Execution;

namespace elasticsearchApi.Utils.InitiatorProcesses
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
            var _cache = services.GetRequiredService<ICacheService>();
            //Initialize Region and District Codes from DB to InMemory
            var list = await _db.Query("address").Select("regionNo", "districtNo").GetAsync<AddressEntity>(cancellationToken: cancellationToken);

            _cache.UpdateObject(CacheKeys.ADDRESS_REFS_KEY, list.ToArray());

            Console.WriteLine($"IN-MEMORY DATA({list.Count()} rows) LOADED SUCCESSFUL!");
        }
        public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
    }
}
