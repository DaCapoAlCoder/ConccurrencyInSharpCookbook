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
using System.Windows.Shapes;

namespace Chapter14_Wpf
{
    /// <summary>
    /// Interaction logic for Chapter14_07_A.xaml
    /// </summary>
    public partial class Chapter14_07_A : Window
    {
        // This class is a problem example rather than any
        // particular exemplar
        public Chapter14_07_A()
        {
            InitializeComponent();
            DataContext = this;
        }


        private string Solve(IProgress<int> progress)
        {
            // Count as quickly as possible for 3 seconds.
            var endTime = DateTime.UtcNow.AddSeconds(3);
            int value = 0;
            while (DateTime.UtcNow < endTime)
            {
                // This is a completely unconstrained loop.
                // The handler/lambda function for the report
                // event is being called so fast that the UI
                // freezes completely for about 20 seconds. Its
                // so slow to update the UI that running this loop
                // for 3 seconds can cause the UI to become unresponsive
                // for a much longer period than the 3 seconds the loop
                // executes. The result is you get the start message
                // and eventually the done message with no updates in
                // between

                // One way to make this work would be to reduce the amount
                // of updates going to the UI, this could be time restrained
                // or using modulus to send updates for only a subset of values (1 in every 100)
                // The problem with this approach is that it can work on the dev
                // machine, but it will respond differently in different environments
                // such as a low power VM or more powerful server. Computing power
                // changes over time so how that approach responds will also change.
                // This is also fixing the problem in the wrong area. It is not the
                // back-end code that should handle the throttling to allow the UI to work
                // but the UI/itself. Solve is running in a background thread rather than
                // in the UI indicating that it is not the correct place to handle the problem

                value++;
                progress?.Report(value);
            }
            return value.ToString();
        }

        // For simplicity, this code updates a label directly.
        // In a real-world MVVM application, those assignments
        //  would instead be updating a ViewModel property
        //  which is data-bound to the actual UI.
        private async void StartButton_Click(object sender, RoutedEventArgs e)
        {
            MyLabel.Content = "Starting...";
            // Progress takes a small lambda that acts as the event handler
            // this will execute every time a progress report is called. In this
            // case the label in the UI should be updated
            var progress = new Progress<int>(value => MyLabel.Content = value);
            // Calls the main program logic passing in the progress instance
            var result = await Task.Run(() => Solve(progress));
            MyLabel.Content = $"Done! Result: {result}";
        }
    }
}
