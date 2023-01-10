﻿using Chapter06;
using Chapter10;
using Chapter10_Wpf;
using Chapter14_Wpf;
using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

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
            //Deadlock();

            //Chapter06_01 chapter06_01 = new();
            //await chapter06_01.Run();

            //Chapter06_02 chapter06_02 = new();
            //chapter06_02.Run();

            //Chapter06_03 chapter06_03 = new();
            //chapter06_03.Run();

            //Chapter06_04 chapter06_04 = new();
            //chapter06_04.Run();

            //Chapter06_05 chapter06_05 = new();
            //chapter06_05.Run();

            //Chapter10_01_Wpf chapter10_01_Wpf = new();
            //chapter10_01_Wpf.Run();

            //Chapter10_06 chapter10_06 = new();
            //chapter10_06.Run();

            //Chapter14_03 chapter14_03 = new();
            //chapter14_03.Run();

            Chapter14_07 chapter14_07 = new();
            chapter14_07.Run();

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
