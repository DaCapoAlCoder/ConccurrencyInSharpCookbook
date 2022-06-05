using Chapter06;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace TestWpf
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }
        private async void Button_Click(object sender, RoutedEventArgs e)
        {
            Chapter06_01 chapter06_1 = new();
            await chapter06_1.Run();

            //Deadlock();
        }
        async Task WaitAsync()
        {
            // This await will capture the current context ...
            await Task.Delay(TimeSpan.FromSeconds(2));
            // ... and will attempt to resume the method here in that context.
        }

        void Deadlock()
        {
            // Start the delay.
            Task task = WaitAsync();

            // Synchronously block, waiting for the async method to complete.
            task.Wait();
            MessageBox.Show("This will show if not deadlocked");
        }
    }
}
