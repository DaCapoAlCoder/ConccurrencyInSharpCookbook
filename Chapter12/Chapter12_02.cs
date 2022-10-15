using Common;
using Nito.AsyncEx;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Chapter12
{
    public class Chapter12_02 : IChapterAsync
    {
        public async Task Run()
        {
            // Asynchronous calls cannot be synchronised with the traditional lock object
            // The semaphore slim has wait and release methods to allow locking of a section
            // of code.

            // The same guidelines apply for async locks as a regular lock
            //  - Restrict lock visibility
            //  - Document what the lock protects
            //  - Minimise the code under the lock
            //  - Never execute arbitrary code while holding a lock (e.g. delegates)
            const int loopCount = 4;
            List<Task> tasks = new List<Task>();
            for (int i = 0; i < loopCount; i++)
            {
                tasks.Add(DelayAndIncrementAsync());
            }

            await Task.WhenAll(tasks);
            if (_value == loopCount)
            {
                Console.WriteLine($"Value is correct: {_value}");
            }
            else
            { 
                Console.WriteLine($"Synchronisation error value is: {_value}");
            }

            // The author claims his async lock has a better API. I'm not so sure
            tasks = new List<Task>();
            _value = 0;
            for (int i = 0; i < loopCount; i++)
            {
                tasks.Add(NitoDelayAndIncrementAsync());
            }

            await Task.WhenAll(tasks);
            if (_value == loopCount)
            {
                Console.WriteLine($"Value is correct: {_value}");
            }
            else
            { 
                Console.WriteLine($"Synchronisation error value is: {_value}");
            }
        }

        // This lock protects the _value field.
        // The constructor value indicates how many threads the lock will allow in
        // Locks should be kept private within the class do not expose them
        private readonly SemaphoreSlim _mutex = new SemaphoreSlim(1);

        private int _value;

        public async Task DelayAndIncrementAsync()
        {
            await _mutex.WaitAsync();
            try
            {
                int oldValue = _value;
                await Task.Delay(TimeSpan.FromSeconds(oldValue));
                _value = oldValue + 1;
            }
            finally
            {
                // Best to put in a finally block to ensure the lock is released
                // even after an exception
                _mutex.Release();
            }
        }

        // This lock protects the _value field.
        private readonly AsyncLock _mutexAsync = new AsyncLock();

        public async Task NitoDelayAndIncrementAsync()
        {
            using (await _mutexAsync.LockAsync())
            {
                int oldValue = _value;
                await Task.Delay(TimeSpan.FromSeconds(oldValue));
                _value = oldValue + 1;
            }
        }
    }
}
