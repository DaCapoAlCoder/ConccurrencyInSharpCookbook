using Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Reactive.Threading.Tasks;
using System.Text;
using System.Threading.Tasks;

namespace Chapter08
{
    public class Chapter08_05 : IChapterAsync
    {
        public async Task Run()
        {
            // Consume observables using await
            await Test();
        }
        async Task Test()
        {
            // Generate the observable
            IObservable<int> observable = Observable.Range(0, 10);

            // Get the last element of an observable stream
            // waits for the stream to complete then returns the
            // last element
            int lastElement = await observable.LastAsync();
            Console.WriteLine($"Last element is: {lastElement}");

            // Awaiting the observable directly implicitly awaits the last element
            int lastElement2 = await observable;
            Console.WriteLine($"Last element is: {lastElement2}");

            // Getting the next element is the same as getting the first element on subscribing
            // to the stream
            int nextElement = await observable.FirstAsync();
            Console.WriteLine($"First element is: {nextElement}");

            IList<int> allElements = await observable.ToList();
            string allElementsCsv = string.Join(",", allElements);
            Console.WriteLine($"All elements: {allElementsCsv}");

            // All of these will subscribe to the observable behind the scenes
            // They unsubscribe when appropriate. FirstAsync() will unsubscribe
            // when the next element is received. The observable stream will need
            // to complete for Last and ToList to work

            // Take and Buffer can be used to get at the elements required without
            // necessarily waiting for the whole stream to complete

            // FirstAsync and LastAsync don't actually return a Task, so using
            // ToTask can be used where a Task is required like Task.WhenAny/All
            // ToTask will only return the last element on completion
        }
    }
}
