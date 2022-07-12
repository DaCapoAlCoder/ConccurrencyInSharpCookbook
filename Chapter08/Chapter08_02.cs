using Common;
using System;
using System.IO;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Chapter08
{
    public class Chapter08_02 : IChapterAsync
    {
        public async Task Run()
        {
            var webRequest = WebRequest.Create("https://www.example.com");

            using var response = await webRequest.GetResponseAsync();
            using var stream = response.GetResponseStream();

            Encoding encode = Encoding.GetEncoding("UTF-8");
            using StreamReader readStream = new StreamReader(stream, encode);
            var end = await readStream.ReadToEndAsync();
            Console.WriteLine(end);
        }
    }

    public static class WebRequestExtensions
    {
        public static Task<WebResponse> GetResponseAsync(this WebRequest client)
        {
            // This use of BeginOperation EndOperation is called the Asynchronous Programming Model(APM)
            // The actual implementation of the client.BeginGetResponse, client.EndGetResponse requires
            // a complicated set-up using callbacks. The best FromAsync overload to use is the
            // begin, end, args and state overload. State was used in asynchronous callbacks before the
            // async method was available in the language. It is no longer used and so it is generally
            // always null. 
            return Task<WebResponse>.Factory.FromAsync(client.BeginGetResponse,
                client.EndGetResponse, null);
        }
    }
}
