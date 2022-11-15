using elasticsearchApi.Utils;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace elasticsearchApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LockController : ControllerBase
    {
        private static readonly ConcurrentLocker locker = new();
        static string logFilePath = "D:\\temp\\lockTest.txt";
        [HttpGet]
        public IActionResult Test()
        {
            try
            {
                int taskCount = 10;
                var tasks = new List<Task>();
                var r = new Random(1);
                for (int i = 0; i < taskCount; i++)
                {
                    string regCode = r.Next(1, 3).ToString();
                    //NewScriptExecutor.WriteLog($"regCode{regCode} | task{i} started at {DateTime.Now:ss.fff}", logFilePath);
                    tasks.Add(Task.Factory.StartNew(() =>
                    {
                        lock (locker[regCode])
                        {
                            NrszService.WriteLog($"regCode{regCode} | task{i} v={v}", logFilePath);
                            //NewScriptExecutor.WriteLog($"regCode{r} | task{idx} locked at {DateTime.Now:ss.fff}", logFilePath);
                            makeLongProcess();
                            //NewScriptExecutor.WriteLog($"regCode{r} | task{idx} released at {DateTime.Now:ss.fff}", logFilePath);
                        }
                    }));
                }
                Task.WaitAll(tasks.ToArray());
                return Ok(new { result = true });
            }
            catch (Exception e)
            {
                return Ok(new { result = false, error = e.GetBaseException().Message, trace = e.StackTrace });
            }
        }
        static int v = 0;
        private void makeLongProcess(int ms = 500)
        {
            Task.Delay(ms).GetAwaiter().GetResult();
            v++;
        }
    }
}
