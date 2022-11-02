using Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Reactive.Threading.Tasks;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Chapter14
{
    public class Chapter14_02 : IChapter
    {
        private CountdownEvent _countdownEvent = new(2);
        public void Run()
        {
            SubscribeWithDefer();
            SubscribeWithoutDefer();

            // One reason to create an async observable that runs for each
            // subscriber would be to call a website each time there is a subscription.
            // Most of the time client code won't subscribe to an observable more than once.
            // Some internal implementations do, such as Observable.While. Re-invoking
            // the observable on each subscription can be useful to refresh data for the subscriber
        }

        void SubscribeWithDefer()
        {
            Console.WriteLine("Using Observable.Defer as a factory will cause the observable to be called for each new subscriber");
            _countdownEvent.Reset();

            var invokeServerObservable = Observable.Defer(
                () => GetValueAsync().ToObservable());
            invokeServerObservable.Subscribe(x => 
            { 
                Console.WriteLine($"Subscribed on thread: {Thread.CurrentThread.ManagedThreadId} using defer the value is {x}"); 
                _countdownEvent.Signal(); 
            });

            invokeServerObservable.Subscribe(x => 
            { 
                Console.WriteLine($"Subscribed on thread: {Thread.CurrentThread.ManagedThreadId} using defer the value is {x}"); 
                _countdownEvent.Signal();  
            });

            _countdownEvent.Wait();
        }

        void SubscribeWithoutDefer()
        {
            Console.WriteLine("Without Using Observable.Defer as a factory the observable is fired only once for each subscriber");
            Console.WriteLine("The result of 13 is still made available to each subscriber in spite of the single invocation of the observable");
            _countdownEvent.Reset();

            var obs = GetValueAsync().ToObservable();
            obs.Subscribe(x => 
            { 
                Console.WriteLine($"Subscribed on thread: {Thread.CurrentThread.ManagedThreadId} without using defer the value is {x}"); 
                _countdownEvent.Signal(); 
            });
            obs.Subscribe(x => 
            { 
                Console.WriteLine($"Subscribed on thread: {Thread.CurrentThread.ManagedThreadId} without using defer the value is {x}"); 
                _countdownEvent.Signal(); 
            });

            _countdownEvent.Wait();
        }

        async Task<int> GetValueAsync()
        {
            Console.WriteLine("Calling server...");
            await Task.Delay(TimeSpan.FromSeconds(2));
            Console.WriteLine("Returning result...");
            return 13;
        }
    }
}
