using System;
using System.Windows;

namespace RotationBug
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

        private void OnButtonPress(object sender, RoutedEventArgs e)
        {
            DisplayText.Text = DateTime.UtcNow.Ticks.ToString();
        }
    }
}
