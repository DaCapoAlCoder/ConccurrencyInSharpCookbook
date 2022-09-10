using Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Chapter10_Wpf
{
    public class Chapter10_06_Wpf : IChapter
    {
        public void Run()
        {
            // Cancelling a subscription is basically just disposing of it
            Win10_06 win = new();
            win.Show();
        }
    }

    class Win10_06 : Window
    {
        private Label MousePositionLabel;

        private IDisposable _mouseMovesSubscription;
        public Win10_06()
        {
            StackPanel stackPanel = new StackPanel { Orientation = Orientation.Vertical };

            var startButton = new Button { Content = "Start Button" };
            var cancelButton = new Button { Content = "Cancel Button" };
            startButton.Click += StartButton_Click;
            cancelButton.Click += CancelButton_Click;

            MousePositionLabel = new();

            stackPanel.Children.Add(startButton);
            stackPanel.Children.Add(cancelButton);
            stackPanel.Children.Add(MousePositionLabel);
            Content = stackPanel;
        }

        private void StartButton_Click(object sender, RoutedEventArgs e)
        {
            IObservable<Point> mouseMoves = Observable
                .FromEventPattern<MouseEventHandler, MouseEventArgs>(
                    handler => (s, a) => handler(s, a),
                    handler => MouseMove += handler,
                    handler => MouseMove -= handler)
                .Select(x => x.EventArgs.GetPosition(this));
            _mouseMovesSubscription = mouseMoves.Subscribe(value =>
            {
                MousePositionLabel.Content = "(" + value.X + ", " + value.Y + ")";
            });
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            if (_mouseMovesSubscription != null)
                _mouseMovesSubscription.Dispose();
        }
    }
}
