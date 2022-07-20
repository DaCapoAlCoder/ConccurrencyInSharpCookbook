using Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Reactive.Linq;
using System.Reactive.Threading.Tasks;
using System.Threading;
using System.Threading.Tasks;

namespace Chapter08
{
    public class Chapter08_06 : IChapter
    {
        public void Run()
        {
            // The main purpose of this section is to take async
            // methods and allow them to be consumed as observables

            HttpClient httpClient = new();

            // The Subscriptions are waiting on different threads
            // the manual reset event forces the main executing 
            // thread to wait for the subscriptions to complete
            // otherwise the program will end before any data is
            // downloaded. The Subscribe method does not act like
            // await, it will not pause the current executing thread
            ManualResetEvent[] manualEvents = {
                new ManualResetEvent(false),
                new ManualResetEvent(false),
                new ManualResetEvent(false),
                new ManualResetEvent(false)
                };

            var dispose1 = GetPageFromTask(httpClient).Subscribe(x =>
            {
                Console.WriteLine("Next");
                Console.WriteLine(x.Substring(0,10));
            },
            x =>
            {
                manualEvents[0].Set();
                Console.WriteLine("Error");
            },
            () =>
            {
                manualEvents[0].Set();
                Console.WriteLine("Complete");
            });

            var dispose2 = GetPageAsHotObservable(httpClient).Subscribe(x =>
            {
                Console.WriteLine("Next");
                Console.WriteLine(x.Substring(0,10));
            },
            x =>
            {
                manualEvents[1].Set();
                Console.WriteLine("Error");
            },
            () =>
            {
                manualEvents[1].Set();
                Console.WriteLine("Complete");
            });

            var dispose3 = GetPageAsColdObservable(httpClient).Subscribe(x =>
            {
                Console.WriteLine("Next");
                Console.WriteLine(x.Substring(0,10));
            },
            x =>
            {
                manualEvents[2].Set();
                Console.WriteLine("Error");
            },
            () =>
            {
                manualEvents[2].Set();
                Console.WriteLine("Complete");
            });

            var urls = new List<string>() 
            { 
                "http://example.com", 
                "http://example.com", 
                "http://example.com" 
            };

            var observable = urls.ToObservable();

            var dispose4 = GetPages(observable, httpClient).Subscribe(x =>
            {
                Console.WriteLine("Next");
                Console.WriteLine(x.Substring(0,20));
            },
            x =>
            {
                manualEvents[3].Set();
                Console.WriteLine("Error");
            },
            () =>
            {
                manualEvents[3].Set();
                Console.WriteLine("Complete");
            });


            // Wait indefinitely for all of the subscriptions to complete
            // and raise set on the manual reset events
            WaitHandle.WaitAll(manualEvents, -1);

            // There's probably a better way to dispose
            dispose1.Dispose();
            dispose2.Dispose();
            dispose3.Dispose();
            dispose4.Dispose();
        }

        IObservable<string> GetPageFromTask(HttpClient client)
        {
            // ToObservable requires the async method to be started
            // and have a task available. Since the download starts
            // with the task, the returned observable doesn't need
            // a subscriber to begin the download process

            Task<string> task =
                client.GetStringAsync("http://www.example.com/");
            return task.ToObservable();
        }

        IObservable<string> GetPageAsHotObservable(HttpClient client)
        {
            // Instead of passing the task in. Start async will start the async
            // method. When this method is called the download is started by the
            // StartAsync method. The observable is hot and doesn't require a 
            // subscriber for the process to start. If the observable is disposed
            // the process will be cancelled via the token passed into the method
            return Observable.StartAsync(
                token => client.GetStringAsync("http://www.example.com/", token));
        }
        IObservable<string> GetPageAsColdObservable(HttpClient client)
        {
            // This method requires a subscriber in order to start the download
            // process. It returns a cold observable. The download can be cancelled
            // by disposing the observable
            return Observable.FromAsync(
                token => client.GetStringAsync("http://www.example.com/", token));
        }

        IObservable<string> GetPages(
            IObservable<string> urls, HttpClient client)
        {
            // This will start a download from each observable that
            // arrives in the stream. This is not the SelectMany from the
            // linq namespace but behaves sort of similarly. 
            return urls.SelectMany(
                (url, token) => client.GetStringAsync(url, token));
        }
    }
}
