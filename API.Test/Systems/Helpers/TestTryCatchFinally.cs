using elasticsearchApi.Tests.Infrastructure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit.Abstractions;

namespace elasticsearchApi.Tests.Systems.Helpers
{
    public class TestTryCatchFinally : TestUtils
    {
        public TestTryCatchFinally(ITestOutputHelper output) : base(output)
        {
        }

        [Fact]
        public void TryCatchFinally_WhenThrows_WillFinally_Invoked()
        {
            Assert.Throws<ApplicationException>(() => CallThrowableOperation());
        }
        private void CallThrowableOperation()
        {
            try
            {
                _output.WriteLine("TRY BLOCK EXECUTED");
                throw new ApplicationException();
            }
            catch (ApplicationException e)
            {
                _output.WriteLine("CATCH BLOCK EXECUTED");
                throw e;
            }
            finally
            {
                _output.WriteLine("FINAL BLOCK EXECUTED");
            }
        }
    }
}
