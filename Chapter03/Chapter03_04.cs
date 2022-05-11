using Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Chapter03
{
    public class Chapter03_04 : IChapterAsync
    {
        public async Task Run()
        {
            await CancelUsingBasicLogic();
            try
            {
                await CancellationWithToken();
            }
            catch
            {
                Console.WriteLine("Cancelled by calling cancel on the token");
            }

            try
            {
                //Even though it is initialized with No Cancellation token, it seems that 
                // the WithCancellationToken method needs to have an enumerable that will take
                // a cancellation token. This will not work on the SlowRange overload that does
                // not take a cancellation token. This is an easy mistake to make

                // This example is a little wonky. Think of the SlowRange method as external
                // library code passing out an Async Enumerable stream, where we don't have
                // access to set the token at the point of the stream being created. Deferred
                // execution means the collection code is not run until the async enumerable collection
                // is actually enumerated (such as in a loop), so we need a way to pass a cancellation token
                // at the point of enumeration. The CanellationToken.None here is used just to select the correct
                // overload in this demo class
                var sequence = SlowRange(CancellationToken.None);
                await ConsumeSequence(sequence);
            }
            catch
            {
                Console.WriteLine("Cancelled the enumerator created by the enumerable");
            }

            try
            {
                var sequence = SlowRange(CancellationToken.None);
                await ConsumeSequenceWithConfigureAwait(sequence);
            }
            catch
            {
                Console.WriteLine("Cancelled. WithCancellation overload does not interfere with ConfigureAwait overload");
            }
        }

        async Task CancelUsingBasicLogic()
        {
            Console.WriteLine("Writing code to break a loop might be all the cancellation required");
            await foreach (int result in SlowRange())
            {
                Console.WriteLine(result);
                if (result >= 4)
                    break;
            }
            Console.WriteLine("Cancelled by using logic to break the loop");

        }

        async Task CancellationWithToken()
        {
            using var cts = new CancellationTokenSource();
            CancellationToken token = cts.Token;
            int index = 0;
            await foreach (int result in SlowRange(token))
            {
                // This kind of cancellation would be better if cancelling async work like an API call
                // Here the async delay is cancelled
                if (++index >= 4)
                {
                    cts.Cancel();
                }
                Console.WriteLine(result);
            }
        }

        async Task ConsumeSequence(IAsyncEnumerable<int> items)
        {
            using var cts = new CancellationTokenSource();
            CancellationToken token = cts.Token;
            // Your code might be passed just the IAsyncEnumerable collection with no ability to set
            // the token where the collection is created, like the above code. There needs to be a
            // way to pass in a cancellation token to the enumerator where it is consumed. This 
            // is done with the WithCancellationToken overload. The deferred execution means the
            // point at which it is consumed is also the point where the enumeration code is run
            int index = 0;
            await foreach (int result in items.WithCancellation(token))
            {
                if (++index >= 4)
                {
                    cts.Cancel();
                }
                Console.WriteLine(result);
            }
        }
        async Task ConsumeSequenceWithConfigureAwait(IAsyncEnumerable<int> items)
        {
            using var cts = new CancellationTokenSource();
            CancellationToken token = cts.Token;
            int index = 0;
            // Can use ConfigureAwait no problem along with WithCancellation
            await foreach (int result in items.WithCancellation(token).ConfigureAwait(false))
            {
                if (++index >= 4)
                {
                    cts.Cancel();
                }
                Console.WriteLine(result);
            }
        }

        // Produce sequence that slows down as it progresses
        async IAsyncEnumerable<int> SlowRange()
        {
            for (int i = 0; i != 10; ++i)
            {
                await Task.Delay(i * 100);
                yield return i;
            }
        }

        // Produce sequence that slows down as it progresses
        async IAsyncEnumerable<int> SlowRange(
            //Note this attribute is required to cancel the enumeration
            [EnumeratorCancellation] CancellationToken token = default)
        {
            for (int i = 0; i != 10; ++i)
            {
                await Task.Delay(i * 100, token);
                yield return i;
            }
        }
    }
}
