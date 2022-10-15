using Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Chapter12
{
    public class Chapter12_03 : IChapterAsync
    {
        public async Task Run()
        {
            // ManualResetEventSlim is the most common cross thread signal
            // Any thread can signal, wait on the signal or even reset the signal
            // These type of controls should only be used for signalling between threads
            // Don't use it:
            //  - If shared data access needs to be synchronised, use a lock instead
            //  - If data needs to be sent between threads, use a producer consumer queue instead

            // Similar signal types exist if ManualResetEventSlim does not meet requirements including
            //  - Barrier
            //  - AutoResetEvent
            //  - CountdownEvent
            var task1 = Task.Run(WaitForInitialization);
            var task2 = Task.Run(InitializeFromAnotherThread);

            await Task.WhenAll(task1, task2);
        }

        private readonly ManualResetEventSlim _initialized =
            new ManualResetEventSlim();

        private int _value;

        public int WaitForInitialization()
        {
            Console.WriteLine($"Value is currently {_value}");
            Console.WriteLine($"Waiting in thread {Thread.CurrentThread.ManagedThreadId} for {nameof(ManualResetEventSlim)} to signal");
            // This will block the current thread until the signal is raised
            _initialized.Wait();
            Console.WriteLine($"Thread {Thread.CurrentThread.ManagedThreadId} finished waiting");
            Console.WriteLine($"Value is currently {_value}");
            return _value;
        }

        public void InitializeFromAnotherThread()
        {
            Console.WriteLine($"Started signalling thread with Id {Thread.CurrentThread.ManagedThreadId}");
            Thread.Sleep(TimeSpan.FromSeconds(5));
            _value = 13;
            _initialized.Set();
            Console.WriteLine($"{nameof(ManualResetEventSlim)} has been signalled in thread {Thread.CurrentThread.ManagedThreadId}");
        }
    }
}
