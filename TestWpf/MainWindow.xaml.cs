using Chapter06;
using System;
using System.Threading.Tasks;
using System.Windows;

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
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Interoperability", "CA1416:Validate platform compatibility", Justification = "<Pending>")]
        private async void Button_Click(object sender, RoutedEventArgs e)
        {
            //Deadlock();

            //Chapter06_01 chapter06_01 = new();
            //await chapter06_01.Run();

            //Chapter06_02 chapter06_02 = new();
            //chapter06_02.Run();

            //Chapter06_03 chapter06_03 = new();
            //chapter06_03.Run();

            Chapter06_04 chapter06_04 = new();
            chapter06_04.Run();
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
