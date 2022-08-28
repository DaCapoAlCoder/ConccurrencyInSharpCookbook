using Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Chapter10
{
    public class Chapter10_00 : IChapterAsync

    {
        public async Task Run()
        {
            // Support for cancellation is co-operative which means it can be
            // requested but not enforced on code. Therefore its goods practice
            // to support cancellation where possible. 

            // Cancellation is split across the source that requests the cancellation
            // and the token that receives and enacts the request. For the calling
            // code to know the cancellation was requested the cancelled code throws
            // an error of type OperationCanceledException. 

            // To show cancellation is supported the code should take a cancellation token
            // which by convention should be the last parameter, unless it also supports
            // progress reporting

            // You can overload a method to allow Cancellation as an option
            // as part of the API
            var task = CancelableMethodWithOverload();
            string body = await task;
            Console.WriteLine($"With overload body was:\n {body}");

            // A default parameter can also be used instead of an overload
            // and populated with the default value for the cancellation token
            // which is the equivalent to CancellationToken.None
            task = CancelableMethodWithDefault();
            body = await task;
            Console.WriteLine($"With default body was:\n {body}");

            // The overloaded method can have a CancellationToken passed in
            CancellationTokenSource cts = new();
            task = CancelableMethodWithOverload(cts.Token);
            cts.Cancel();
            body = await task;
            Console.WriteLine($"With overload body was:\n {body}");

            // The default parameter can also be over written with a cancellation
            // token which can be used to cancel the request 
            cts = new();
            task = CancelableMethodWithDefault(cts.Token);
            cts.Cancel();
            body = await task;
            Console.WriteLine($"With default body was:\n {body}");
        }

        public async Task<string> CancelableMethodWithOverload(CancellationToken cancellationToken)
        {
            try
            {
                HttpClient client = new();
                var response = await client.GetAsync("https://www.example.com", cancellationToken);
                return await response.Content.ReadAsStringAsync();

            }
            catch (OperationCanceledException)
            {
                return "Cancelled";
            }
        }

        public async Task<string> CancelableMethodWithOverload()
        {
            return await CancelableMethodWithOverload(CancellationToken.None);
        }

        public async Task<string> CancelableMethodWithDefault(
            CancellationToken cancellationToken = default)
        {
            return await CancelableMethodWithOverload(cancellationToken);
        }
    }
}
