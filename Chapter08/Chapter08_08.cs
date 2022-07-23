using Common;
using System;
using System.Linq;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

namespace Chapter08
{
    public class Chapter08_08 : IChapter
    {
        public void Run()
        {
            ManualResetEvent manualResetEvent = new(false);
            AdaptDataflowMeshToObservable(manualResetEvent);
            WaitHandle.WaitAny(new[] { manualResetEvent });

            UseObservableAsInputToDataFlowMesh();
        }

        void AdaptDataflowMeshToObservable(ManualResetEvent manualResetEvent)
        {
            var buffer = new BufferBlock<int>();
            // As observable will return an observable version of the mesh
            IObservable<int> integers = buffer.AsObservable();
            integers.Subscribe(
                data => Console.WriteLine(data),
                ex => Console.WriteLine(ex),
                () =>
                {
                    manualResetEvent.Set();
                    Console.WriteLine("Done");
                });

            buffer.Post(13);
            buffer.Complete();
        }
        void UseObservableAsInputToDataFlowMesh()
        {
            // Create an observable that acts as an input to the mesh
            IObservable<DateTimeOffset> ticks =
                Observable.Interval(TimeSpan.FromSeconds(1))
                    .Timestamp()
                    .Select(x => x.Timestamp)
                    .Take(5);

            // Set up the TPL dataflow mesh
            var display = new ActionBlock<DateTimeOffset>(x => Console.WriteLine(x));

            // The action block can be changed to an observer and passed in to the subscriber
            ticks.Subscribe(display.AsObserver());

            try
            {
                // Here we wait for the data flow block to complete
                // for some reason using await here does not wait for the mesh to complete
                display.Completion.Wait();
                Console.WriteLine("Done.");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }
    }
}
