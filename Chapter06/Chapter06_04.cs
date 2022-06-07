using Common;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace Chapter06
{
    public class Chapter06_04 : IChapter
    {
        public void Run()
        {
            Win win = new();
            win.Show();
            //win.Throttle();
            win.Sample();
        }

        class Win : Window
        {
            public void Throttle()
            {

                Observable.FromEventPattern<MouseEventHandler, MouseEventArgs>(
                        handler => (s, a) => handler(s, a),
                        handler => MouseMove += handler,
                        handler => MouseMove -= handler)
                    .Select(x => x.EventArgs.GetPosition(this))
                    // Won't emit until an event stream has started and then stopped for 1 second
                    // Good for auto-complete type situtations, where the user must stop typing before 
                    // making a call
                    .Throttle(TimeSpan.FromSeconds(1))
                    .Take(3)
                    .Subscribe(x => Trace.WriteLine(
                        $"{DateTime.Now.Second}: Saw {x.X + x.Y}"));
            }

            public void Sample()
            {
                Observable.FromEventPattern<MouseEventHandler, MouseEventArgs>(
                        handler => (s, a) => handler(s, a),
                        handler => MouseMove += handler,
                        handler => MouseMove -= handler)
                    .Select(x => x.EventArgs.GetPosition(this))
                    // Sample just waits for the timespan after the stream values arrives,
                    // samples the value and emits it. This means it does not wait for input
                    // to stop. It just sends whatever data element is passing through at the
                    // given time intervl
                    .Sample(TimeSpan.FromSeconds(1))
                    .Take(3)
                    .Subscribe(x => Trace.WriteLine(
                        $"{DateTime.Now.Second}: Saw {x.X + x.Y}"));
            }
        }
    }
}
