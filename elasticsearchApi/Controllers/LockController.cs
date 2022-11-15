using elasticsearchApi.Utils;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
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
                Enumerable.Range(1, 1000)
                    .AsParallel()
                    .ForAll(async i => await ManageConcurrency($"{i % 2}", async () => await Task.Delay(TimeSpan.FromSeconds(10))));
                return Ok(new { result = true });
            }
            catch (Exception e)
            {
                return Ok(new { result = false, error = e.GetBaseException().Message, trace = e.StackTrace });
            }
        }

        private static async Task<bool> ManageConcurrency(string taskId, Func<Task> task)
        {
            Lazy<SemaphoreSlim> taskLock = locker[taskId];
            Console.WriteLine($"CurrentCount:{taskLock.Value.CurrentCount}");
            try
            {
                if (taskLock.Value.CurrentCount == 0)
                {
                    Console.WriteLine($"{DateTime.Now:hh:mm:ss.ffffff},  {taskId}, I didn't find, and then found/created. None available.. Thread Id: {Thread.CurrentThread.ManagedThreadId}");
                    return false;
                }
                else
                {
                    taskLock.Value.Wait(TimeSpan.FromSeconds(1));

                    Console.WriteLine($"{DateTime.Now:hh:mm:ss.ffffff},  {taskId}, I didn't find, then found/created, and took. Thread Id: {Thread.CurrentThread.ManagedThreadId}");
                }

                Console.WriteLine($"{DateTime.Now:hh:mm:ss.ffffff},  {taskId}, Lock pulled for TaskId {taskId}, Thread Id: {Thread.CurrentThread.ManagedThreadId}");

                await task.Invoke();

                return true;
            }
            catch (Exception e)
            {
                ;
                return false;
            }
            finally
            {
                //taskLock?.Release();
                //locker._dictionary.Remove(taskId, out _);
                //Console.WriteLine($"I released. Thread Id: {Thread.CurrentThread.ManagedThreadId}");
            }
        }
    }
}
