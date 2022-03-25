using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace Chapter2
{
    public class Chapter2_4
    {
        public async Task Run()
        {
            await FromDelays();
            await FromResults();
            await DownloadAllAsync(new HttpClient(), new string[] { "http://www.googl.com", "http://www.google.com", "http://www.google.com" });
            await ObserveOneExceptionAsync();
            await ObserveAllExceptionsAsync();
        }

        async Task FromDelays()
        {
            Console.WriteLine("Tasks with delays for 1,2 and 3 seconds");
            Task task1 = Task.Delay(TimeSpan.FromSeconds(1));
            Task task2 = Task.Delay(TimeSpan.FromSeconds(2));
            Task task3 = Task.Delay(TimeSpan.FromSeconds(3));

            await Task.WhenAll(task1, task2, task3);
            Console.WriteLine("TaskWhen All took 3 seconds to finish");
        }

        async Task FromResults()
        {
            // Only tasks of the same result type will return a value from WhenAll
            // If the tasks are of mixed type Task<int> & Task<double> then it will
            // return a Task instead of a Task<T>
            Console.WriteLine("3 new Tasks using Task.FromResult(), WhenAll will return an array of values");
            Task<int> task1 = Task.FromResult(3);
            Task<int> task2 = Task.FromResult(5);
            Task<int> task3 = Task.FromResult(7);

            int[] results = await Task.WhenAll(task1, task2, task3);
            Console.WriteLine("Task results:");
            foreach (int result in results)
            {
                Console.WriteLine($"Task results: {result}");
            }
        }
        async Task<string> DownloadAllAsync(HttpClient client, IEnumerable<string> urls)
        {
            // This creates an array of Tasks.
            // The tasks have not started yet because of deferred execution.
            var downloads = urls.Select(url => client.GetStringAsync(url));

            // This forces the collection to be evaluated and so the tasks 
            // will start here.
            Task<string>[] downloadTasks = downloads.ToArray();
            // Now the tasks have all started.

            // Asynchronously wait for all downloads to complete.
            // The tasks all return the same Task<string> type so WhenAll can
            // return a value.
            string[] htmlPages = await Task.WhenAll(downloadTasks);

            //There is an overload for WhenAll that will take an IEnumerable of tasks (below).
            //Author thinks mixing async and linq is not as clean as forcing execution
            //of the collection first and then passing that to overload that takes:
            //"params Task<TResult>[]" tasks as an overload to be more clear. The
            //implementation below using IEnumerable works the same way as the one above
            string[] htmlPages2 = await Task.WhenAll(downloads);


            string threeWebPages = string.Concat(htmlPages);
            Console.WriteLine("First 500 chars of three google home pages:");
            Console.WriteLine(threeWebPages.Substring(0, 500));

            return threeWebPages;
        }

        async Task ThrowNotImplementedExceptionAsync()
        {
            await Task.CompletedTask;
            throw new NotImplementedException();
        }

        async Task ThrowInvalidOperationExceptionAsync()
        {
            await Task.CompletedTask;
            throw new InvalidOperationException();
        }

        async Task ObserveOneExceptionAsync()
        {
            //Both of these exceptions will occur in WhenAll.
            //But only one of them will actually be thrown by WhenAll            
            // Not that the methods do not throw directly but use the async keyword
            // so that their exceptions will be added to a Task that is return normally
            // Removing async keyowrd would cause the first exception to be thrown on
            // the line below rather than in WhenAll
            var task1 = ThrowNotImplementedExceptionAsync();
            var task2 = ThrowInvalidOperationExceptionAsync();

            // This is another way to create a Task that contains an exception
            task1 = Task.FromException(new NotImplementedException());
            task2 = Task.FromException(new InvalidOperationException());

            try
            {
                await Task.WhenAll(task1, task2);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Two Exceptions occurred in WhenAll. The one thrown by it was was: {ex.GetType()}");
            }
        }

        async Task ObserveAllExceptionsAsync()
        {
            var task1 = ThrowNotImplementedExceptionAsync();
            var task2 = ThrowInvalidOperationExceptionAsync();

            // In order to see all exception details the tasks must first be
            // retrieved from WhenAll and then awaited, rather than awaiting it
            // directly.
            Task allTasks = Task.WhenAll(task1, task2);

            try
            {
                await allTasks;
            }
            catch
            {
                //The exceptions are contained in the inner exception
                // of the aggregate
                AggregateException allExceptions = allTasks.Exception;

                Console.WriteLine("Two exceptions occurred in WhenAll, awaiting the task instead of the method wraps them in an Aggregate Exception");
                Console.WriteLine("The exception that occured were:");
                foreach(var ex in allExceptions.InnerExceptions)
                {
                    Console.WriteLine($"{ex.GetType()}");
                }
            }
        }
    }
}
