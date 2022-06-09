using Microsoft.Reactive.Testing;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chapter07
{
    [TestClass]
    public class Chaper07_06
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

            public IObservable<string> GetStringWithTimeout(string url,
                IScheduler scheduler = null)
            {
                return _httpService.GetString(url)
                    // In order to make this method testable, the schedular is required
                    // so that it can be mocked out during testing, this avoids waiting
                    // for the actual timeout to complete in realtime
                    .Timeout(TimeSpan.FromSeconds(1), scheduler ?? Scheduler.Default);
            }
        }

        private class SuccessHttpServiceStub : IHttpService
        {
            public IScheduler Scheduler { get; set; }
            public TimeSpan Delay { get; set; }

            public IObservable<string> GetString(string url)
            {
                return Observable.Return("stub")
                    .Delay(Delay, Scheduler);
            }
        }

        [TestMethod]
        public void MyTimeoutClass_SuccessfulGetShortDelay_ReturnsResult()
        {
            // The test scheduler is part of the reactive testing nuget package
            var scheduler = new TestScheduler();
            var stub = new SuccessHttpServiceStub
            {
                Scheduler = scheduler,
                // Here the delay needs to be less than the timeout from the TimeoutClass
                // If the stub is set to a time greater than the timeout it will not take
                // that amount of time to execute the test because of the Test Scheduler
                Delay = TimeSpan.FromSeconds(0.8),
            };
            var myTimeoutClass = new MyTimeoutClass(stub);
            string result = null;

            myTimeoutClass.GetStringWithTimeout("http://www.example.com/", scheduler)
                .Subscribe(r => { result = r; });

            scheduler.Start();

            Assert.AreEqual("stub", result);
        }

        [TestMethod]
        public void MyTimeoutClass_SuccessfulGetLongDelay_ThrowsTimeoutException()
        {
            var scheduler = new TestScheduler();
            var stub = new SuccessHttpServiceStub
            {
                Scheduler = scheduler,
                // This test does not take 1.5 seconds to execute
                Delay = TimeSpan.FromSeconds(1.5),
            };
            var my = new MyTimeoutClass(stub);
            Exception result = null;

            my.GetStringWithTimeout("http://www.example.com/", scheduler)
                .Subscribe(_ => Assert.Fail("Received value"), 
                // uses the exception action of the subscriber to get the exception
                ex => { result = ex; });

            scheduler.Start();

            Assert.IsInstanceOfType(result, typeof(TimeoutException));
        }
    }
}
