using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit.Abstractions;

namespace elasticsearchApi.Tests.Infrastructure
{
    public abstract class TestUtils
    {
        public readonly ITestOutputHelper _output;
        public TestUtils(ITestOutputHelper output)
        {
            _output = output;
        }
    }
}
