using Common;
using System;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Reactive.Linq;
using System.Reactive.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace Chapter06
{
    public class Chapter06_05 : IChapter
    {
        public void Run()
        {
            //GetWithTimeout(new HttpClient(), "http://www.example.com/");
            //GetWithTimeout(new HttpClient(), "https://microsoft.com");

            Win win = new();
            win.Show();
            //win.MouseTimeout();
            win.TrackMouseMovesTimeoutTrackClicks();
        }

        void GetWithTimeout(HttpClient client, string url)
        {
            client.GetStringAsync(url).ToObservable()
                // If no events are received for the given timespan the observable will timeout
                .Timeout(TimeSpan.FromSeconds(1))
                .Subscribe(
                    x => Trace.WriteLine($"{DateTime.Now.Second}: Saw {x.Length}"),
                    // Captures the timeout exception here
                    ex => Trace.WriteLine(ex));
        }

        class Win : Window
        {
            public void MouseTimeout()
            {

                Observable.FromEventPattern<MouseEventHandler, MouseEventArgs>(
                        handler => (s, a) => handler(s, a),
                        handler => MouseMove += handler,
                        handler => MouseMove -= handler)
                    .Select(x => x.EventArgs.GetPosition(this))
                    .Timeout(TimeSpan.FromSeconds(1))
                    .Subscribe(
                        x => Trace.WriteLine($"{DateTime.Now.Second}: Saw {x.X + x.Y}"),
                        // Captures the timeout exception here
                        ex => Trace.WriteLine(ex));
            }
            public void TrackMouseMovesTimeoutTrackClicks()
            {
                IObservable<Point> clicks =
                    Observable.FromEventPattern<MouseButtonEventHandler, MouseButtonEventArgs>(
                        handler => (s, a) => handler(s, a),
                        handler => MouseDown += handler,
                        handler => MouseDown -= handler)
                    .Select(x => x.EventArgs.GetPosition(this));
                //There's no subscribe here so this won't start first

                Observable.FromEventPattern<MouseEventHandler, MouseEventArgs>(
                        handler => (s, a) => handler(s, a),
                        handler => MouseMove += handler,
                        handler => MouseMove -= handler)
                    .Select(x => x.EventArgs.GetPosition(this))
                    // In the case of a timeout the clicks sequence will be passed instead of the
                    // mouse event
                    .Timeout(TimeSpan.FromSeconds(1), clicks)
                    .Subscribe(
                        x => Trace.WriteLine($"{DateTime.Now.Second}: Saw {x.X},{x.Y}"),
                        ex => Trace.WriteLine(ex));
            }

        }

    }
}
