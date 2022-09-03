using Common;
using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace Chapter10_Wpf
{
    public class Chapter10_01_Wpf : IChapter
    {
        private Button StartButton;
        private Button CancelButton;
        private CancellationTokenSource _cts;

        public void Run()
        {
            // A more "realistic" example of cancellation. The Start button starts a delay
            // and assigns the CancelationTokenSource. The cancel button will invoke the
            // the cancellation of the delay. The Start process can end in the three ways
            // success, cancellation or error

            StackPanel stackPanel = new StackPanel { Orientation = Orientation.Vertical };
            StartButton = new Button { Content = "Start Button" };
            CancelButton = new Button { Content = "Cancel Button", IsEnabled = false };
            StartButton.Click += StartButton_Click;
            CancelButton.Click += CancelButton_Click;
            stackPanel.Children.Add(StartButton);
            stackPanel.Children.Add(CancelButton);

            Win win = new Win();
            win.Content = stackPanel;
            win.Show();
        }

        public async void StartButton_Click(object sender, RoutedEventArgs e)
        {
            StartButton.IsEnabled = false;
            CancelButton.IsEnabled = true;
            try
            {
                _cts = new CancellationTokenSource();
                CancellationToken token = _cts.Token;
                await Task.Delay(TimeSpan.FromSeconds(5), token);
                MessageBox.Show("Delay completed successfully.");
            }
            catch (OperationCanceledException)
            {
                MessageBox.Show("Delay was cancelled.");
            }
            catch (Exception)
            {
                MessageBox.Show("Delay completed with error.");
                throw;
            }
            finally
            {
                StartButton.IsEnabled = true;
                CancelButton.IsEnabled = false;
            }
        }

        public void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            _cts.Cancel();
            CancelButton.IsEnabled = false;
        }
    }

    class Win : Window
    {
        public Win()
        {
        }
    }
}
