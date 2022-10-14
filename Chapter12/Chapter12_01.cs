using Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Chapter12
{
    public class Chapter12_01 : IChapterAsync
    {
        public async Task Run()
        {
            // There are multiple kinds of lock including
            //  - Monitor
            //  - SpinLock
            //  - ReaderWriterLockSlim
            // 99% of cases can be handled with the use of a lock object

            // The guidelines for using a lock are:
            //  - Restrict Lock Visibility
            //  - Document what a lock protects
            //  - Minimize the code within the lock
            //  - Never execute arbitrary code while holding a lock (events, execute delegate, virtual methods)

            // The lock object should always be private and never exposed outside of the class
            // Use one lock per type(class) and if you need more than one consider refactoring
            // You can lock on anything but its better to use a dedicated object.
            // Locking this means the lock object is publicly exposed and can cause deadlocks

            // Ideally the code should not make blocking calls when under a lock. Blocking call would be
            // GetAwaiter().GetResult() Not sure why that recommendation is made maybe this leads to
            // deadlock, it might just increase the time the lock is used)

            // Executing arbitrary code means you don't can't control what's happening in the lock
            // do it after the lock is released.

            var task1 = Task.Run(() => Increment());
            var task2 = Task.Run(() => Increment());
            var task3 = Task .Run(() => Increment());
            var task4 = Task .Run(() => Increment());

            await Task.WhenAll(task1, task2, task3, task4);

            Console.WriteLine($"Value is {_value}");
        }

        // This lock protects the _value field.
        private readonly object _mutex = new object();

        private int _value;

        public async Task Increment()
        {
            await Task.Delay(100);
            lock (_mutex)
            {
                _value = _value + 1;
            }
        }
    }
}
