using Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chapter02
{
    public class Chapter02_08 : IChapterAsync
    {
        public async Task Run()
        {
            await ThrowExceptionAwaitingMethod();
            await ThrowExceptionAwaitingTask();

        }
        async Task ThrowExceptionAsync()
        {
            await Task.Delay(TimeSpan.FromSeconds(1));
            throw new InvalidOperationException("Test");
        }

        async Task ThrowExceptionAwaitingMethod()
        {
            try
            {
                Console.WriteLine("Exception throwing method awaited");
                await ThrowExceptionAsync();
            }
            catch (InvalidOperationException ex)
            {
                Console.WriteLine($"Exception type {(ex.GetType())} caught");
            }
        }

        async Task ThrowExceptionAwaitingTask()
        {
            // The exception is thrown by the method and placed on the task.
            Task task = ThrowExceptionAsync();
            Console.WriteLine($"Task for exception throwing method has started with status: {task.Status}");
            try
            {
                Console.WriteLine("Calling method is still executing before awaiting the tasks");
                // The exception is re-raised here, where the task is awaited.
                await task;
            }
            catch (InvalidOperationException ex)
            {
                Console.WriteLine($"The task is awaited an exception of {ex.GetType()} was caught");
            }
        }
    }
}
