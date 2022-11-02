using Common;
using Nito.AsyncEx;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Chapter14
{
    public class Chapter14_01 : IChapterAsync
    {
        public async Task Run()
        {
            // It doesn't matter how many times and from how many threads
            // UseSharedInteger is called it will only initialize once
            var task1 = Task.Run(UseSharedInteger);
            var task2 = Task.Run(UseSharedInteger);
            var task3 = Task.Run(UseSharedInteger);
            await Task.WhenAll(task1, task2, task3);

            task1 = Task.Run(GetSharedIntegerAsync);
            task2 = Task.Run(GetSharedIntegerAsync);
            task3 = Task.Run(GetSharedIntegerAsync);
            await Task.WhenAll(task1, task2, task3);

            AsyncContext.Run(async () =>
            {
                // When accessing from different contexts (two old asp.net requests or a UI/Background thread)
                // then it may be better to perform the initialisation on a thread pool thread instead of the
                // context. The author doesn't go into detail as to why that makes a difference
                Console.WriteLine($"Getting shared value from \"UI\" context {Thread.CurrentThread.ManagedThreadId}");
                task1 = GetSharedIntegerAsyncOnBackgroundThread();
                task2 = Task.Run(GetSharedIntegerAsyncOnBackgroundThread);
                task3 = Task.Run(GetSharedIntegerAsyncOnBackgroundThread);
                await Task.WhenAll(task1, task2, task3);
            });

            task1 = Task.Run(GetMySharedIntegerWithErrorHandlingAsync);
            task2 = Task.Run(GetMySharedIntegerWithErrorHandlingAsync);
            task3 = Task.Run(GetMySharedIntegerWithErrorHandlingAsync);
            try
            {
                await Task.WhenAll(task1, task2, task3);
            }
            catch (Exception)
            {

            }

            // The Nito library has its own implementation of the LazyAsync class 
            task1 = Task.Run(UseSharedIntegerNitoAsync);
            task2 = Task.Run(UseSharedIntegerNitoAsync);
            task3 = Task.Run(UseSharedIntegerNitoAsync);
            await Task.WhenAll(task1, task2, task3);
        }

        #region lazy initialisation 
        // Static will allow only a single instance of the value in code
        static int _simpleValue;
        static readonly Lazy<int> MySharedInteger = new Lazy<int>(() =>
        {
            Console.WriteLine($"Initializing once in thread {Thread.CurrentThread.ManagedThreadId}");
            // The value returned by the backing field setting the value for the Lazy type,
            // before it's incremented, so MySharedInteger.Value will always return 0
            return _simpleValue++;
        });

        void UseSharedInteger()
        {
            int sharedValue = MySharedInteger.Value;
            Console.WriteLine($"Accessing shared value from thread {Thread.CurrentThread.ManagedThreadId} value is {sharedValue}");
        }
        #endregion 

        #region lazy initialisation with async workload
        // This is exactly the same as above but the lazy initialisation involves an async workload
        static int _simpleValueAsync;
        static readonly Lazy<Task<int>> MySharedAsyncInteger =
            new Lazy<Task<int>>(async () =>
            {
                Console.WriteLine($"Initializing once in thread {Thread.CurrentThread.ManagedThreadId}");
                await Task.Delay(TimeSpan.FromSeconds(2)).ConfigureAwait(false);
                return _simpleValueAsync++;
            });

        async Task GetSharedIntegerAsync()
        {
            int sharedValue = await MySharedAsyncInteger.Value;
            Console.WriteLine($"Accessing shared value from thread {Thread.CurrentThread.ManagedThreadId} value is {sharedValue}");
        }
        #endregion

        #region lazy initialisation with async workload to be shared between different contexts
        static int _simpleValueAsyncOnBackgroundThread;
        static readonly Lazy<Task<int>> MySharedAsyncIntegerOnBackgroundThread =
          new Lazy<Task<int>>(() => Task.Run(async () =>
          {
              // Running this inside of Task.Run ensures that the initialisation is handled in
              // a background thread instead of the UI context
              Console.WriteLine($"Initializing once in a background thread {Thread.CurrentThread.ManagedThreadId} instead of UI context");
              await Task.Delay(TimeSpan.FromSeconds(2));
              // The problem here is that if the async workload throws an exception then the exception will be 
              // cashed in the Lazy's task rather than the actual value and there is no recourse for recovery
              // in this configuration
              return _simpleValueAsyncOnBackgroundThread++;
          }));

        async Task GetSharedIntegerAsyncOnBackgroundThread()
        {
            int sharedValue = await MySharedAsyncIntegerOnBackgroundThread.Value;
            Console.WriteLine($"Accessing shared value from non UI thread {Thread.CurrentThread.ManagedThreadId} value is {sharedValue}");
        }
        #endregion

        #region Lazy initialisation with error handling
        public sealed class AsyncLazy<T>
        {
            private readonly object _mutex;
            private readonly Func<Task<T>> _factory;
            private Lazy<Task<T>> _instance;

            public AsyncLazy(Func<Task<T>> factory)
            {
                _mutex = new object();
                _factory = RetryOnFailure(factory);
                _instance = new Lazy<Task<T>>(_factory);
            }

            private Func<Task<T>> RetryOnFailure(Func<Task<T>> factory)
            {
                // This function will execute the factory async used to get
                // the actual value (such as retrieving a value from a database)
                return async () =>
                {
                    try
                    {
                        // This code only executes when the Value property of .
                        // the Lazy instance is actually accessed, not on initialisation
                        // If it throws the factory is reassigned to the Lazy instance allowing
                        // if to be retried by accessing the instance again outside of the class.
                        // This is better than the previous example which would just store the
                        // exception in lazy
                        Random random = new();

                        // Throw an exception once in a while to demo the error handling
                        if (random.Next(0, 2) == 0)
                        {
                            throw new InvalidOperationException();
                        }

                        Console.WriteLine($"Successfully initialised once on thread: {Thread.CurrentThread.ManagedThreadId}");
                        return await factory().ConfigureAwait(false);
                    }
                    catch
                    {
                        Console.WriteLine($"Failed to initialise on thread: {Thread.CurrentThread.ManagedThreadId}");

                        // This executes arbitrary code under a lock which breaks one
                        // of the authors own guidelines. It is locked because it is
                        // assigning to _instance, while the Task property below reads
                        // from it. There's probably no need to lock in the constructor
                        // as only one thread will construct an instance
                        lock (_mutex)
                        {
                            _instance = new Lazy<Task<T>>(_factory);
                        }

                        throw;
                    }
                };
            }

            // This is just a property exposing the task. Reading and awaiting
            // this value will execute the initialisation code and retry logic
            // above
            public Task<T> Task
            {
                get
                {
                    lock (_mutex)
                    {
                        return _instance.Value;
                    }
                }
            }
        }

        // These are just the same lazy inits from the previous examples except they use
        // the AsyncLazy class
        static int _simpleIntValue;
        static readonly AsyncLazy<int> MySharedAsyncLazyInteger =

          new AsyncLazy<int>(() => Task.Run(async () =>
          {
              await Task.Delay(TimeSpan.FromSeconds(2));
              return _simpleIntValue++;
          }));

        async Task GetMySharedIntegerWithErrorHandlingAsync()
        {
            Console.WriteLine($"Getting value in thread: {Thread.CurrentThread.ManagedThreadId}");
            int sharedValue = await MySharedAsyncLazyInteger.Task;
        }
        #endregion

        #region Lazy initialisation using the Nito library
        static int _simpleValueNito;
        private static readonly Nito.AsyncEx.AsyncLazy<int> MySharedAsyncIntegerNito =
          new Nito.AsyncEx.AsyncLazy<int>(async () =>
          {
               Console.WriteLine($"Initializing once in thread {Thread.CurrentThread.ManagedThreadId}");
              await Task.Delay(TimeSpan.FromSeconds(2));
              return _simpleValueNito++;
          },
          // The flag argument allows similar error handling behaviour to the
          // by-hand implementation above
          AsyncLazyFlags.RetryOnFailure);

        public async Task UseSharedIntegerNitoAsync()
        {
            int sharedValue = await MySharedAsyncIntegerNito;
            Console.WriteLine($"Accessing shared value from thread {Thread.CurrentThread.ManagedThreadId} value is {sharedValue}");
        }
        #endregion
    }
}

