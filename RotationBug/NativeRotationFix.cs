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
    class NativeRotationFix
    {
        private readonly Window window;

        private const string MessageWindowTitle = "SystemResourceNotifyWindow";
        private const uint WM_DISPLAYCHANGE = 0x007E;
        private const int Delay = 500;

        private int width;
        private int height;
        private int depth;

        public delegate bool WNDENUMPROC(IntPtr hWnd, IntPtr lParam);

        [DllImport("user32.dll")]
        public static extern bool EnumThreadWindows(uint dwThreadId, WNDENUMPROC lpfn, IntPtr lParam);

        [DllImport("user32.dll")]
        public static extern int GetWindowText(IntPtr hWnd, StringBuilder lpString, int nMaxCount);

        [DllImport("user32.dll")]
        public static extern bool PostMessage(IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam);

        [DllImport("kernel32.dll")]
        static extern uint GetCurrentThreadId();

        public NativeRotationFix(Window window)
        {
            this.window = window;
            SystemEvents.DisplaySettingsChanged += OnDisplaySettingsChanged;
        }

        ~NativeRotationFix()
        {
            SystemEvents.DisplaySettingsChanged -= OnDisplaySettingsChanged;
        }

        private void OnDisplaySettingsChanged(object sender, EventArgs e)
        {
            new DispatcherTimer(TimeSpan.FromMilliseconds(Delay), DispatcherPriority.Normal, (s, a) =>
            {
                WindowInteropHelper interopHelper = new WindowInteropHelper(window);

                Screen screen = Screen.FromHandle(interopHelper.Handle);
                width = screen.Bounds.Width;
                height = screen.Bounds.Height;
                depth = screen.BitsPerPixel;

                uint threadId = GetCurrentThreadId();
                EnumThreadWindows(threadId, PostToNotifyWindow, IntPtr.Zero);

                (s as DispatcherTimer).Stop();

            }, Dispatcher.CurrentDispatcher);
        }

        private bool PostToNotifyWindow(IntPtr hwnd, IntPtr lparam)
        {
            StringBuilder buffer = new StringBuilder(MessageWindowTitle.Length + 1);

            if (GetWindowText(hwnd, buffer, buffer.Capacity) <= 0) return true;
            if (buffer.ToString() != MessageWindowTitle) return true;

            PostMessage(hwnd, WM_DISPLAYCHANGE, new IntPtr(depth), new IntPtr(MakeLong(width, height)));
            return false;
        }

        private static int MakeLong(int low, int high)
        {
            return (int)((ushort)low | (uint)high << 16);
        }
    }
}
