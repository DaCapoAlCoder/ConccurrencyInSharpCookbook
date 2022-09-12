using Common;
using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace Chapter11
{
    public class Chapter11_01 : IChapterAsync
    {
        public async Task Run()
        {
            // One main point from the chapter is that it is the run types that are
            // awaitable and not the method. So, the use of async is an implementation detail 
            // so it may require awaiting in the implementation or it may just return a task to
            // be awaited elsewhere
            
            MyAsyncClass myAsyncClass = new();
            await UseMyInterfaceAsync(new HttpClient(), myAsyncClass);

            MyAsyncClassStub myAsyncClassStub = new();
            int bytes = await myAsyncClassStub.CountBytesAsync(new HttpClient(), "http://example.com");
            Console.WriteLine($"Stubbed bytes: {bytes}");

            MyConcreteAsyncClass myConcreteAsyncClass = new();
            int exampleBytes = await myConcreteAsyncClass.CountBytesAsync(new HttpClient(), "http://example.com");
            Console.WriteLine($"Bytes from concrete implementation: {exampleBytes}");
        }
        interface IMyAsyncInterface
        {
            // The async element is an implementation detail. The implementation
            // can just return a task without awaiting it. Therefore, it would not
            // need the async keyword. Likewise the implementation could await a
            // task which would require the async keyword. Either way, it is not
            // something that should be in the interface description of the method
            Task<int> CountBytesAsync(HttpClient client, string url);
        }

        class MyAsyncClass : IMyAsyncInterface
        {
            public async Task<int> CountBytesAsync(HttpClient client, string url)
            {
                var bytes = await client.GetByteArrayAsync(url);
                return bytes.Length;
            }
        }

        async Task UseMyInterfaceAsync(HttpClient client, IMyAsyncInterface service)
        {
            var result = await service.CountBytesAsync(client, "http://www.example.com");
            Console.WriteLine(result);
        }

        // The implementation doesn't have to be asynchronous, it can be synchronous
        // as in the case below
        class MyAsyncClassStub : IMyAsyncInterface
        {
            public Task<int> CountBytesAsync(HttpClient client, string url)
            {
                return Task.FromResult(13);
            }
        }

        // Like the interface the abstract class doesn't specify the use of the async
        // keyword, as it is an implementation detail
        abstract class MyAsyncAbstractClass
        {
            public abstract Task<int> CountBytesAsync(HttpClient client, string url);

        }

        class MyConcreteAsyncClass : MyAsyncAbstractClass
        {
            public override async Task<int> CountBytesAsync(HttpClient client, string url)
            {
                var bytes = await client.GetByteArrayAsync(url);
                return bytes.Length;
            }
        }
    }
}
