using Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chapter2
{
    public class Chapter2_10 : IChapter, IAsyncDisposable
    {

        public async Task Run()
        {
            // Almost always use Task<T> as the return type for your method.
            // ValueTask is a potential performance optimisation that should be verified as faster using a profiler 
            // Use value task where most of the time the method can return a value synchronously.
            // This avoids allocating an actual Task on synchronous execution
            // The code will run slower when executing asynchronously than a method returning Task<T> because ValueTask actually wraps a Task<T>
            // It seems like this sort of optimisation would only be useful in fast executing loops or high throughput code paths
            // There are more advanced scenarios 
            await ValueTaskMethodAsync();
            Console.WriteLine(await MethodAsync(true));
            Console.WriteLine(await MethodAsync(false));

            await DisposeAsync();
            await DisposeAsync();
        }
        public async ValueTask<int> ValueTaskMethodAsync()
        {
            Console.WriteLine("Returning a ValueTask works the same as returning from a Task, just await it");
            await Task.Delay(100); // asynchronous work.
            return 13;
        }


        public ValueTask<string> MethodAsync(bool CanBehaveSynchronously)
        {
            if (CanBehaveSynchronously)
            {
                Console.WriteLine("Running method with a value generated synchronously");
                return new ValueTask<string>("synchronously generated/cached value: 13");
            }
            Console.WriteLine("Running method with a value generated asynchronously");
            return new ValueTask<string>(SlowMethodAsync());
        }

        private Task<string> SlowMethodAsync() => Task.FromResult("Slower executing async method value: 13");

        public ValueTask DisposeAsync()
        {
            // This method only runs the async dispose logic once
            // every other time it just returns the default ValueTask

            if (_disposeLogic == null)
            {
                Console.WriteLine("Dispose has been called a second time");
                return default;
            }

            // Note: this simple example is not thread safe;
            //  if multiple threads call DisposeAsync,
            //  the logic could run more than once.
            Func<Task> logic = _disposeLogic;
            Console.WriteLine("Setting dispose logic to null so it won't be executed twice");
            // Assigning null here does not set Logic to null 
            _disposeLogic = null;
            return new ValueTask(logic());
        }

        private Func<Task> _disposeLogic = () =>
        {
            Console.WriteLine("Disposing Asynchronously");
            return Task.Delay(TimeSpan.FromSeconds(2));
        };
    }
}
