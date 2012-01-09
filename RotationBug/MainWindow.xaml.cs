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
        private int width;
        private int height;
        private int depth;

        [DllImport("user32.dll", SetLastError = true)]
        public static extern bool EnumThreadWindows(uint dwThreadId, WNDENUMPROC lpfn, IntPtr lParam);

        public delegate bool WNDENUMPROC(IntPtr hWnd, IntPtr lParam);

        [DllImport("user32.dll", SetLastError = true)]
        public static extern int GetWindowText(IntPtr hWnd, StringBuilder lpString, int nMaxCount);

        [DllImport("user32.dll", SetLastError = true)]
        public static extern bool PostMessage(IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam);

        [DllImport("kernel32.dll")]
        static extern uint GetCurrentThreadId();

        public MainWindow()
        {
            InitializeComponent();

            SystemEvents.DisplaySettingsChanged += OnDisplaySettingsChanged;
        }

        ~MainWindow()
        {
            SystemEvents.DisplaySettingsChanged -= OnDisplaySettingsChanged;
        }

        private void OnDisplaySettingsChanged(object sender, EventArgs e)
        {
            new DispatcherTimer(TimeSpan.FromMilliseconds(500), DispatcherPriority.Normal, (s, a) =>
            {
                WindowInteropHelper interopHelper = new WindowInteropHelper(this);

                Screen screen = Screen.FromHandle(interopHelper.Handle);
                width = screen.Bounds.Width;
                height = screen.Bounds.Height;
                depth = screen.BitsPerPixel;

                uint threadId = GetCurrentThreadId();
                EnumThreadWindows(threadId, GetSystemResourceNotifyWindow, IntPtr.Zero);

                (s as DispatcherTimer).Stop();

            }, Dispatcher.CurrentDispatcher);
        }

        private bool GetSystemResourceNotifyWindow(IntPtr hwnd, IntPtr lparam)
        {
            const int capacity = 100;
            StringBuilder buffer = new StringBuilder(capacity);

            if (GetWindowText(hwnd, buffer, buffer.Capacity) > 0)
            {
                if (buffer.ToString().Equals("SystemResourceNotifyWindow", StringComparison.InvariantCultureIgnoreCase))
                {
                    int val = (int)(((ushort) width) | (((uint) height) << 16));
                    IntPtr ptr = new IntPtr(val);
                    PostMessage(hwnd, 0x007E, new IntPtr(depth), ptr);
                    return false;
                }
            }

            return true;
        }

        private void OnButtonPress(object sender, RoutedEventArgs e)
        {
            DisplayText.Text = DateTime.UtcNow.Ticks.ToString();
        }
    }
}
