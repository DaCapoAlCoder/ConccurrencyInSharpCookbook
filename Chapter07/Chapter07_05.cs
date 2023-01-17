using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chapter07
{
    [TestClass]
    public class Chapter07_05
    {
        public interface IHttpService
        {
            IObservable<string> GetString(string url);
        }

        public class MyTimeoutClass
        {
            private readonly IHttpService _httpService;

            public MyTimeoutClass(IHttpService httpService)
            {
                _httpService = httpService;
            }

            public IObservable<string> GetStringWithTimeout(string url)
            {
                return _httpService.GetString(url)
                    .Timeout(TimeSpan.FromSeconds(1));
            }
        }

        // This stub would be better created with something like Moq
        class SuccessHttpServiceStub : IHttpService
        {
            public IObservable<string> GetString(string url)
            {
                // This is a cold observable that returns a single value
                // These types of observables are useful for testing
                return Observable.Return("stub");
            }
        }

        [TestMethod]
        public async Task MyTimeoutClass_SuccessfulGet_ReturnsResult()
        {
            var stub = new SuccessHttpServiceStub();
            // Pass the stub in and validate the result is returned
            var my = new MyTimeoutClass(stub);

            var result = await my.GetStringWithTimeout("http://www.example.com/")
                // Single expects to return a single element from an observable and will throw if there is more than one
                .SingleAsync();

            Assert.AreEqual("stub", result);
        }

        private class FailureHttpServiceStub : IHttpService
        {
            public IObservable<string> GetString(string url)
            {
                // Like observable return its a cold observable that will throw an exception
                return Observable.Throw<string>(new HttpRequestException());
            }
        }

        [TestMethod]
        public async Task MyTimeoutClass_FailedGet_PropagatesFailure()
        {
            var stub = new FailureHttpServiceStub();
            var my = new MyTimeoutClass(stub);

            // This would be better accomplished with Assert.ThrowsExceptionAsync
            await Chapter07_02.ThrowsAsync<HttpRequestException>(async () =>
            {
                await my.GetStringWithTimeout("http://www.example.com/")
            .SingleAsync();
            });
        }
    }
}
