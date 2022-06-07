using Common;
using System;
using System.Diagnostics;
using System.Linq;
using System.Reactive.Linq;
using System.Windows;
using System.Windows.Input;

namespace Chapter06
{
    public class Chapter06_03 : IChapter
    {
        public void Run()
        {
            BufferData();
            WindowData();

            BufferWin bufferWin = new();
            bufferWin.Show();
            bufferWin.BufferForTimeSpan();
        }
        void BufferData()
        {
            Observable.Interval(TimeSpan.FromSeconds(1))
                .Take(8)
                .Buffer(2)
                // x is a list the size of the buffer
                .Subscribe(x => Trace.WriteLine(
                    // Won't execute until the buffer is full
                    $"{DateTime.Now.Second}: Got {x[0]} and {x[1]}"));
        }
        void WindowData()
        {
            Observable.Interval(TimeSpan.FromSeconds(1))
                .Take(8)
                // A window unlike a buffer will start producing results immediatly
                // The outter subscription ends once the window size has been filled
                .Window(2)
                .Subscribe(group =>
                {
                    Trace.WriteLine($"{DateTime.Now.Second}: Starting new group");
                    // there is an in subscription that will process data coming into the window
                    // immediatly
                    group.Subscribe(
                x => Trace.WriteLine($"{DateTime.Now.Second}: Saw {x}"),
                () => Trace.WriteLine($"{DateTime.Now.Second}: Ending group"));
                });
        }

        class BufferWin : Window
        {
            public void BufferForTimeSpan()
            {
                Observable.FromEventPattern<MouseEventHandler, MouseEventArgs>(
                        handler => (s, a) => handler(s, a),
                        handler => MouseMove += handler,
                        handler => MouseMove -= handler)
                    // Buffer and Window both work with timespan, allowing the buffer or window
                    // to accumulate for a set period of time
                    .Buffer(TimeSpan.FromSeconds(1))
                    .Take(5)
                    .Subscribe(x => Trace.WriteLine(
                        $"{DateTime.Now.Second}: Saw {x.Count} items."));
            }
        }
    }
}
