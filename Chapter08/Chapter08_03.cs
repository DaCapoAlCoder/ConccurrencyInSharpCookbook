using Common;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Chapter08
{
    public class Chapter08_03 : IChapterAsync
    {
        public async Task Run()
        {
            AsyncHttpService asyncHttpService = new();

            // This way would be the old school way to get the result of an asynchronous action, using a callback
            string downloadedValue = string.Empty;
            asyncHttpService.DownloadString("https://www.example.com", (result, exception) => { downloadedValue = result; });
            Console.WriteLine(downloadedValue);

            // This extension method uses the call back to set the downloaded string to the value on a task completion source which then returns the
            // the task. This allows the await operator to be used instead of the callback. Its a nice way to handle and modernise old library code
            var downloaded = await asyncHttpService.DownloadStringAsync("https://www.example.com");
            Console.WriteLine(downloaded);

            // In this instance exceptions are caught explicitly, but that might not always be the case. It may be necessary to catch exceptions
            // and other errors within the call back so that they can be used to set the task completion source result. Its important that the
            // task completion source always completes which means error handling is very important and must be thought through. This technique
            // can be used to wrap any asynchronous method
        }
    }

    public class RequestState
    {
        public WebRequest request;
        public Action<string, Exception> callback;
        public ManualResetEvent manualResetEvent;
    }

    public interface IMyAsyncHttpService
    {
        void DownloadString(string address, Action<string, Exception> callback);
    }

    public class AsyncHttpService : IMyAsyncHttpService
    {
        ManualResetEvent manualResetEvent = new ManualResetEvent(false);
        public void DownloadString(string address, Action<string, Exception> callback)
        {
            RequestState requestState = new();
            var webRequest = WebRequest.Create(address);
            requestState.request = webRequest;
            requestState.callback = callback;
            requestState.manualResetEvent = manualResetEvent;

            IAsyncResult result = webRequest.BeginGetResponse(new AsyncCallback(RespCallback), requestState);
            const int DefaultTimeout = 2 * 60 * 1000; // 2 minutes timeout
            // this line implements the timeout, if there is a timeout, the callback fires and the request becomes aborted
            ThreadPool.RegisterWaitForSingleObject(result.AsyncWaitHandle, new WaitOrTimerCallback(TimeoutCallback), webRequest, DefaultTimeout, true);

            // The response came in the allowed time. The work processing will happen in the
            // callback function.
            manualResetEvent.WaitOne();
        }

        private static void TimeoutCallback(object state, bool timedOut)
        {
            if (timedOut)
            {
                HttpWebRequest request = state as HttpWebRequest;
                if (request != null)
                {
                    request.Abort();
                }
            }
        }

        private static void RespCallback(IAsyncResult asynchronousResult)
        {
            Exception caught = null;
            var requestState = (RequestState)asynchronousResult.AsyncState;
            string result = string.Empty;
            try
            {
                // State of request is asynchronous.
                WebRequest myHttpWebRequest = requestState.request;
                var response = (HttpWebResponse)myHttpWebRequest.EndGetResponse(asynchronousResult);

                // Synchronously read the data to avoid more callback implementations
                using var stream = response.GetResponseStream();
                Encoding encode = Encoding.GetEncoding("UTF-8");
                using StreamReader readStream = new StreamReader(stream, encode);
                result = readStream.ReadToEnd();

                // Read the response into a Stream object.
                return;
            }
            catch (Exception e)
            {
                caught = e;
            }
            finally
            {
                requestState.callback.Invoke(result, caught);
                requestState.manualResetEvent.Set();
            }
        }
    }

    public static class AsyncHttpServiceExensions
    {
        public static Task<string> DownloadStringAsync(
            this IMyAsyncHttpService httpService, string address)
        {
            var tcs = new TaskCompletionSource<string>();
            httpService.DownloadString(address, (result, exception) =>
            {
                if (exception != null)
                    tcs.TrySetException(exception);
                else
                    tcs.TrySetResult(result);
            });
            return tcs.Task;
        }
    }
}
