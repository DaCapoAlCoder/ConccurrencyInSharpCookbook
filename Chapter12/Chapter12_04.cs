using Common;
using Nito.AsyncEx;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chapter12
{
    public class Chapter12_04 : IChapterAsync
    {
        public async Task Run()
        {
            // The TaskCompletionsource allows signals to be sent between threads asynchronously
            // Unlike ManualResetEventSlim, waiting for the TaskCompletionSource does not block the
            // current thread. So the wait task can be started while the calling thread can continue
            // allowing the task to be awaited later.

            // As with the synchronous version, only use this type of signal for actual signalling 
            // between threads and not synchronising data access or sharing data between threads

            // This type of signal is good for one shot signal but it does not work if you need to turn
            // the signal on and off.
            var task = WaitForInitializationAsync();
            Console.WriteLine("Delay started");
            await Task.Delay(TimeSpan.FromSeconds(4));
            Console.WriteLine("Delay done");
            Initialize();
            await task;

            // The AsyncManualResetEvent operates as an asynchronous signal but allows the signal to
            // be reset as well as being set.
            var task2 = WaitForConnectedAsync();
            Console.WriteLine("Delay started");
            await Task.Delay(TimeSpan.FromSeconds(2));
            Console.WriteLine("Delay done");
            ConnectedChanged(true);
            await task2;

            Console.WriteLine("\n\nRunning the same AsyncManualResetEvent again");
            ConnectedChanged(false);
            task2 = WaitForConnectedAsync();
            Console.WriteLine("Delay started");
            await Task.Delay(TimeSpan.FromSeconds(4));
            Console.WriteLine("Delay done");
            ConnectedChanged(true);
            await task2;

        }

        private readonly TaskCompletionSource<object> _initialized =
            new TaskCompletionSource<object>();

        private int _value1;
        private int _value2;

        public async Task<int> WaitForInitializationAsync()
        {
            Console.WriteLine("Start waiting for async signal");
            await _initialized.Task;
            Console.WriteLine("Signal received");
            return _value1 + _value2;
        }

        public void Initialize()
        {
            Console.WriteLine("Sending signal");
            _value1 = 13;
            _value2 = 17;
            _initialized.TrySetResult(null);
        }

        private readonly AsyncManualResetEvent _connected =
            new AsyncManualResetEvent();

        public async Task WaitForConnectedAsync()
        {
            Console.WriteLine("Start waiting for async signal");
            await _connected.WaitAsync();
            Console.WriteLine("Signal received");
        }

        public void ConnectedChanged(bool connected)
        {
            if (connected)
            {
                Console.WriteLine("Sending signal");
                _connected.Set();
            }
            else
            {
                Console.WriteLine("Resetting signal");
                _connected.Reset();
            }
        }
    }
}
