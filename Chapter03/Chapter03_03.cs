using Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chapter03
{
    public class Chapter03_03 : IChapterAsync
    {
        public async Task Run()
        {
            // Can use the method ToAsyncEnumerable, and can use WhereAwait SelectAwait and others
            // This is a System.Linq.Async package is a contributor package and not part of corefx
            // The documentation is non-existent. Finding the available operations can be done by F12ing 
            // a method like WhereAwait

            // Async Linq methods can end in Await or Async. Operators ending in await take an asynchronous delegate.
            // This means they actually perform an await operation in the delegate. The operators ending in Async return
            // a Task that represents a value, rather than something that represents a asynchronous stream.
            await TestWhereAwait();
            await TestWhere();
            await TestCountAsnyc();
            await TestCountAwaitAsync();
        }

        async Task TestWhereAwait()
        {
            Console.WriteLine("Using WhereAwait, make sense if where requires async operation");
            IAsyncEnumerable<int> values = SlowRange().WhereAwait(
                async value =>
                {
                    // do some asynchronous work to determine
                    //  if this element should be included

                    // In this case it might be fetching data from a database
                    // or API to compare to the value of WhereAwait Input parameter
                    await Task.Delay(10);
                    return value % 2 == 0;
                });

            await foreach (int result in values)
            {
                Console.WriteLine(result);
            }
        }

        async Task TestWhere()
        {
            // Produce sequence that slows down as it progresses
            Console.WriteLine("Using just Where, no async work is required so synchronous Where is fine");
            // No async work is required in the Where statement, the stream can still be processed
            // asynchronously
            IAsyncEnumerable<int> values = SlowRange().Where(
            value => value % 2 == 0);

            await foreach (int result in values)
            {
                Console.WriteLine(result);
            }
        }
        async Task TestCountAsnyc()
        {
            int count = await SlowRange().CountAsync(
            value => value % 2 == 0);

            Console.WriteLine($"Count Async is a terminal operation with an Async suffix, it cannot take an async predicate. The value is {count}");
        }

        async Task TestCountAwaitAsync()
        {
            int count = await SlowRange().CountAwaitAsync(
                async value =>
                {
                    await Task.Delay(10);
                    return value % 2 == 0;
                });
            Console.WriteLine($"Count Await Async is both terminal and allows async predicates. The value is {count}");
        }

        async IAsyncEnumerable<int> SlowRange()
        {
            for (int i = 0; i != 10; ++i)
            {
                await Task.Delay(i * 100);
                yield return i;
            }
        }
    }
}
