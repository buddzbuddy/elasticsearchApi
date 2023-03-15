using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace elasticsearchApi.Tests.Infrastructure
{
    public abstract class SettingsControllerTestBase : TestBase
    {
        protected Task RunTest(Func<HttpClient, Task> test, string? environmentName = null, IDictionary<string, string>? configuration = null)
        {
            return RunTestInternal(
                    services => Task.CompletedTask,
                    client => test(client),
                    services => Task.CompletedTask,
                    environmentName,
                    configuration
                );
        }

        protected override void ConfigureTestServices(IServiceCollection services) { }
        protected override IEnumerable<IDbContextTransaction> InitializeTransactions(IServiceProvider services)
            => Array.Empty<IDbContextTransaction>();
    }
}
