using Common;
using Nito;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

namespace Chapter14
{
    public class Chapter14_06 : IChapterAsync
    {
        public async Task Run()
        {
            // Normal behaviour for a data flow mesh is to stop processing
            // when it experiences an exception. One way to allow processing to
            // continue on exception, is to use a type that represents either an
            // actual value or an exception and pass that through the mesh. The author
            // calls this railway programming because the code executes either on a
            // value track or an error track. Dubious metaphor.
            await Test();
        }

        private static TransformBlock<Try<TInput>, Try<TOutput>> RailwayTransform<TInput, TOutput>(Func<TInput, TOutput> func)
        {
            // The point of this code is to create a transform block that calls the map function of
            // the Try type. Instead of the transform block calling the function used to transform data,
            // the Map function is used to call that function. If an exception is thrown by the
            // transform function the Map function will map that exception to the exception section of
            // the Try type, otherwise the returned value from the function will be placed into the 
            // value section of the Try type. Exceptions are not exposed to the underlying Data Flow
            // type so processing can continue
            return new TransformBlock<Try<TInput>, Try<TOutput>>(t => t.Map(func));
        }

        async Task Test()
        {
            // Either the result of the functions passed in will get mapped to the Try
            // type or else the corresponding exception
            var subtractBlock = RailwayTransform<int, int>(value => value - 2);
            var divideBlock = RailwayTransform<int, int>(value => 60 / value);
            var multiplyBlock = RailwayTransform<int, int>(value => value * 2);

            var options = new DataflowLinkOptions { PropagateCompletion = true };
            subtractBlock.LinkTo(divideBlock, options);
            divideBlock.LinkTo(multiplyBlock, options);

            // Insert data items into the first block
            subtractBlock.Post(Try.FromValue(5));
            // this one will throw a divide by zero
            subtractBlock.Post(Try.FromValue(2));
            subtractBlock.Post(Try.FromValue(4));
            subtractBlock.Complete();

            // Receive data/exception items from the last block
            while (await multiplyBlock.OutputAvailableAsync())
            {
                Try<int> item = await multiplyBlock.ReceiveAsync();
                // At the output of the mesh the type are checked for
                // value which will be true for a value and false for 
                // an exception. item.IsException could also be used
                if (item.IsValue)
                {
                    Console.WriteLine(item.Value);
                }
                else
                { 
                    Console.WriteLine(item.Exception.Message);
                }
            }
        }
    }
}
