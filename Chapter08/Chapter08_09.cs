using Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace Chapter08
{
    public class Chapter08_09 : IChapterAsync
    {
        public async Task Run()
        {
            // In the following two methods if the producer creates at a 
            // rate faster than the consumer can consumer, the buffer will
            // keep growing, and the system can run out of resources. Assuming
            // the resources being bounded memoryGH

            await ToObservable();

            await ToObservableUsingUnboundedExtension();

            // This implementation won't run out of memory but will drop elements
            // if the buffer is full. 
            await ToObservableUsingBoundedExtension();

            // The main point here is observables are push constructs and streams are pull
            // constructs, so they are opposing ideas. The issue where the producer creates
            // more than an a consumer must be handled in some manner such as here by
            // consuming more resources or by dropping items when the buffer is full

            // Back pressure is a way to handle a producer that is faster than a consumer
            // but there is no pattern implementation for it in the Reactive extensions
            // right now.
        }

        private static async Task ToObservableUsingUnboundedExtension()
        {
            IObservable<long> observable =
                Observable.Interval(TimeSpan.FromSeconds(1)).Take(3);

            var enumerable = observable.ToAsyncEnumerableUnbounded();
            await foreach (var yoke in enumerable)
            {
                Console.WriteLine(yoke);
            }
        }

        private static async Task ToObservableUsingBoundedExtension()
        {
            IObservable<long> observable =
                Observable.Interval(TimeSpan.FromMilliseconds(250)).Take(9);

            var enumerable = observable.ToAsyncEnumerableBounded(1);
            await foreach (var yoke in enumerable)
            {
                await Task.Delay(1000);
                Console.WriteLine(yoke);
            }
        }

        async Task ToObservable()
        {
            IObservable<long> observable =
                Observable.Interval(TimeSpan.FromSeconds(1)).Take(3);

            // WARNING: May consume unbounded memory; see discussion!
            IAsyncEnumerable<long> enumerable =
                observable.ToAsyncEnumerable();

            await foreach (var yoke in enumerable)
            {
                Console.WriteLine(yoke);
            }
        }
    }

    public static class ObservableExtnesions
    {
        // This is basically an implementation of the built in ToAsyncEnumerable
        // method above

        // WARNING: May consume unbounded memory; see discussion!
        public static async IAsyncEnumerable<T> ToAsyncEnumerableUnbounded<T>(
            this IObservable<T> observable)
        {
            // The idea is to subscribe the observable stream and push the
            // // items to a queue (Channel), then pull the values from the queue
            // // into an async  to the observable stream and push the
            // items to a queue (Channel), then pull the values from the queue
            // into an async stream

            // Channel is basically a queue
            Channel<T> buffer = Channel.CreateUnbounded<T>();

            // subscribe to the observable and add items to the channel
            using (observable.Subscribe(
                value => buffer.Writer.TryWrite(value),
                error => buffer.Writer.Complete(error),
                () => buffer.Writer.Complete()))
            {
                // This section is just the using block

                // Read back out of the channel and yield the value to 
                // create the async stream
                await foreach (T item in buffer.Reader.ReadAllAsync())
                    yield return item;
            }
        }

        // WARNING: May discard items; see discussion!
        public static async IAsyncEnumerable<T> ToAsyncEnumerableBounded<T>(
            this IObservable<T> observable, int bufferSize)
        {
            // Sets up options to drop the oldest element when the buffer
            // is full at the given size
            var bufferOptions = new BoundedChannelOptions(bufferSize)
            {
                FullMode = BoundedChannelFullMode.DropOldest,
            };

            // Creates a bounded channel of the given buffer size
            Channel<T> buffer = Channel.CreateBounded<T>(bufferOptions);
            using (observable.Subscribe(
                // Same as above subscribe and write to the channel on next
                value => buffer.Writer.TryWrite(value),
                error => buffer.Writer.Complete(error),
                () => buffer.Writer.Complete()))
            {
                // Read from the channel and yield to create the async stream
                await foreach (T item in buffer.Reader.ReadAllAsync())
                    yield return item;
            }
        }
    }
}
