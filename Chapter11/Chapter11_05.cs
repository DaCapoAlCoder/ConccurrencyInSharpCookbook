using Common;
using Nito.AsyncEx;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chapter11
{
    public class Chapter11_05 : IChapterAsync
    {
        public async Task Run()
        {
            // The author refers to two types of events notifications and commands.
            // Notification events are fire and forget where the event raiser does not
            // care if any actions have taken place as a result or even if there are any
            // handlers/listeners registered with it. These type of events can exist in
            // async void methods. The async void method won't know when a handler completes
            // but it also doesn't care. 

            // Command events are not strictly events they are used to trigger some process
            // in another part of the code base. The command events raiser must wait for the
            // handler to complete its work before continuing in its execution path. 

            // Universal Windows Platform apps have a similar concept built in called Deferrals
            // this library is probably used for converting old code to async

            // The event itself works like any with handlers that can be registered as normal
            MyEvent += AsyncHandler;
            MyEvent += async (object sender, MyEventArgs args) => {
                using var deferral = args.GetDeferral();
                await Task.Delay(TimeSpan.FromSeconds(1));
                Console.WriteLine("Hello"); 
            };
            await RaiseMyEventAsync();
            Console.WriteLine("Continued");
        }

        public class MyEventArgs : EventArgs, IDeferralSource
        {
            private readonly DeferralManager _deferrals = new DeferralManager();

            // ... // Your own constructors and properties.

            // This is a Nito library for implementing deferrals in the event argument
            // class. The event handler will get the deferral in its method while the 
            // event raiser, will then await that deferral. This forces the code to wait
            // for the handler to complete before continuing the main code essentially enforcing
            // an async event
            public IDisposable GetDeferral()
            {
                return _deferrals.DeferralSource.GetDeferral();
            }

            internal Task WaitForDeferralsAsync()
            {
                return _deferrals.WaitForDeferralsAsync();
            }
        }

        public event EventHandler<MyEventArgs> MyEvent;

        async Task RaiseMyEventAsync()
        {
            
            EventHandler<MyEventArgs> handler = MyEvent;
            if (handler == null)
            { 
                return;
            }

            var args = new MyEventArgs();
            handler(this, args);

            // Awaiting here waits for the deferrals to complete. When the deferrals
            // are all disposed their counts internally go to zero and the count event behind
            // the scenes will allow code to continue past this point
            await args.WaitForDeferralsAsync();
        }

        // Notice that the async handler is still and async void type event handler
        async void AsyncHandler(object sender, MyEventArgs args)
        {
            // This line must be present in order for the wait method to work
            // if it is removed the handler will not act like an awaited async
            // method. Rather it will act like an async method that is not awaited.

            // Behind the scenes a count down event is used. It is incremented once
            // for every GetDeferral and decremented once every time a deferral is disposed
            // The count down event has a wait all that waits for the count to be zero. This
            // means that all handlers have fully awaited their async workloads 
            using IDisposable deferral = args.GetDeferral();
            await Task.Delay(TimeSpan.FromSeconds(2));
            Console.WriteLine("Handled");
        }
    }
}
