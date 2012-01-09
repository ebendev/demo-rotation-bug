using System;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Interop;
using System.Windows.Threading;
using Microsoft.Win32;

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

            new NativeRotationFix(this);
        }

        private void OnButtonPress(object sender, RoutedEventArgs e)
        {
            DisplayText.Text = DateTime.UtcNow.Ticks.ToString();
        }
    }
}
