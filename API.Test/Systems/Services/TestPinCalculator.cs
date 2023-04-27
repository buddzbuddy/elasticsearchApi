using elasticsearchApi.Tests.Infrastructure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit.Abstractions;

namespace elasticsearchApi.Tests.Systems.Services
{
    public class TestPinCalculator : TestUtils
    {
        public TestPinCalculator(ITestOutputHelper output) : base(output)
        {
        }

        [Fact]
        public void CalculateMaxIIN_WhenCalled_Returns_MAX_IIN()
        {

        }
    }
}
