using elasticsearchApi.Data.Seed;
using elasticsearchApi.Models;
using SqlKata.Execution;

namespace elasticsearchApi.Utils.InitiatorProcesses
{
    public class SeedAddressData : IHostedService
    {
        // We need to inject the IServiceProvider so we can create 
        // the scoped service, MyDbContext
        private readonly IServiceProvider _serviceProvider;
        public SeedAddressData(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            // Create a new scope to retrieve scoped services
            using var scope = _serviceProvider.CreateScope();
            var services = scope.ServiceProvider;
            var _db = services.GetRequiredService<QueryFactory>();
            //Initialize Region and District Codes from DB to InMemory
            if(await _db.Query("address").CountAsync<int>(cancellationToken: cancellationToken) > 0)
            {
                Console.WriteLine("STARTUP SQL ADDRESS UP TO DATE");
                return;
            }
            Console.WriteLine("STARTUP SQL ADDRESS INSERTING STARTED...");
            foreach (var x in DataSeeder.addressEntities)
                await _db.Query("address").InsertAsync(x);
            Console.WriteLine("STARTUP SQL ADDRESS INSERTING FINISHED");
        }
        public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
    }
}
