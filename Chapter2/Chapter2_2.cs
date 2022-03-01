using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Chapter2
{
    public class Chapter2_2
    {
        public async Task Run()
        {
            
            var resultValue = await GetValueFromResult();
            Console.WriteLine($"Value returned by Task from result:{resultValue}");

            var task = GetCompletedTask();
            Console.WriteLine($"This is the task is completed it hasn't been awaited. It status is: {task.Status}");
            await task;
            Console.WriteLine($"Awaiting a completed task has no affect");

            var cancelledTask = GetCancelled(true);
            Console.WriteLine($"This is the task is cancelled it hasn't been awaited. Its status is: {cancelledTask.Status}");
            try
            {
                await cancelledTask;
            }
            catch (TaskCanceledException e)
            {
                Console.WriteLine($"It throws an exception when awaited: {e.Message}");
            }

            try
            {
                await FromException();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }

            try
            {

                Console.WriteLine("This class throws an exception in a synchronous method");
                Console.WriteLine("The exception is caught and returned asynchronously using from exception");
                await HandleException();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        public Task<int> GetValueFromResult()
        {
            return new MySynchronousImplementationFromResult().GetValueAsync();
        }

        // no async here we don't await on completed tasks or task statuses
        public Task GetCompletedTask()
        {
            return new MySynchronousImplementationCompletedTask().DoSomethingAsync();
        }


        public Task<int> GetCancelled(bool cancel = false)
        {
            var cts = new CancellationTokenSource();
            if (cancel)
            {
                cts.Cancel();
            }

            return new MySynchronousImplementationFromCancelled().GetValueAsync(cts.Token);
        }

        public Task<int> FromException()
        {
            return new MySynchronousImplmentationFromException().NotImplementedAsync<int>();
        }

        public Task HandleException()
        {
            return new MySynchronousImplementationHandleException().DoSomethingAsync(true);
        }


        interface IMyAsyncInterfaceFromResult
        {
            Task<int> GetValueAsync();
        }

        class MySynchronousImplementationFromResult : IMyAsyncInterfaceFromResult
        {
            public Task<int> GetValueAsync()
            {
                return Task.FromResult(13);
            }
        }

        interface IMyAsyncInterfaceCompletedTask
        {
            Task DoSomethingAsync();
        }

        class MySynchronousImplementationCompletedTask : IMyAsyncInterfaceCompletedTask
        {
            public Task DoSomethingAsync()
            {
                return Task.CompletedTask;
            }
        }

        class MySynchronousImplementationFromCancelled
        {
            public Task<int> GetValueAsync(CancellationToken cancellationToken)
            {
                if (cancellationToken.IsCancellationRequested)
                    return Task.FromCanceled<int>(cancellationToken);
                return Task.FromResult(13);
            }
        }

        class MySynchronousImplmentationFromException
        {
            public Task<T> NotImplementedAsync<T>()
            {
                return Task.FromException<T>(new NotImplementedException());
            }
        }


        interface IMyAsyncInterfaceHandleException
        {
            Task DoSomethingAsync(bool throwException);
        }

        class MySynchronousImplementationHandleException : IMyAsyncInterfaceHandleException
        {
            static void DoSomethingSynchronously(bool throwException = false)
            {
                if (throwException)
                {
                    throw new Exception();
                }
            }

            public Task DoSomethingAsync(bool throwException = false)
            {
                try
                {
                    DoSomethingSynchronously(throwException);
                    return Task.CompletedTask;
                }
                catch (Exception ex)
                {
                    return Task.FromException(ex);
                }
            }
        }
    }
}
