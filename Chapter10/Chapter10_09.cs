using Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Chapter10
{
    public class Chapter10_09 : IChapterAsync
    {
        public async Task Run()
        {
            try
            {
                using CancellationTokenSource cts = new();
                cts.Cancel();
                await PingAsync("www.google.com", cts.Token);
            }
            catch (Exception)
            { 
                Console.WriteLine("Operation cancelled");
            }
        }

        async Task<PingReply> PingAsync(string hostNameOrAddress, CancellationToken cancellationToken)
        {
            using var ping = new Ping();
            Task<PingReply> task = ping.SendPingAsync(hostNameOrAddress);
            // By registering a callback with the cancellation token, its possible to interface with other 
            // APIs that have some for of cancellation mechanism but don't use the standard Cancellation Token
            // Here the ping method just has a SendAsyncCancel method used to cancel the ping request

            // Note the cancellation registration is disposable
            using CancellationTokenRegistration _ = cancellationToken.Register(() => CallBackWrapper(ping));
            return await task;
        }

        void CallBackWrapper(Ping ping)
        {
            Console.WriteLine("Token cancelled, executing callback");
            ping.SendAsyncCancel();
        }
    }
}
