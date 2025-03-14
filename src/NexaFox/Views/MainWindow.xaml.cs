using System.Runtime.InteropServices;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Shell;
using Microsoft.Web.WebView2.Core;
using NexaFox.ViewModels;


namespace NexaFox.Views
{
    public partial class MainWindow : Window
    {


        public MainWindow()
        {
            InitializeComponent();
            InitializeResize();
        }
        
        public void InitializeResize()
        {
            var windowChrome = new WindowChrome
            {
                ResizeBorderThickness = new Thickness(8), 
                CaptionHeight = 0,
                CornerRadius = new CornerRadius(8),
                GlassFrameThickness = new Thickness(0)
            };
            WindowChrome.SetWindowChrome(this, windowChrome);
        }

        


        private void ResizeWindow(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton != MouseButton.Left) return;

            var rectangle = sender as Rectangle;
            var tag = rectangle?.Tag.ToString();

            var handle = new WindowInteropHelper(this).Handle;
            ReleaseCapture();

            switch (tag)
            {
                case "TopLeft":
                    SendMessage(handle, 0x112, (IntPtr)13, IntPtr.Zero); // WMSZ_TOPLEFT
                    break;
                case "Top":
                    SendMessage(handle, 0x112, (IntPtr)12, IntPtr.Zero); // WMSZ_TOP
                    break;
                case "TopRight":
                    SendMessage(handle, 0x112, (IntPtr)14, IntPtr.Zero); // WMSZ_TOPRIGHT
                    break;
                case "Left":
                    SendMessage(handle, 0x112, (IntPtr)10, IntPtr.Zero); // WMSZ_LEFT
                    break;
                case "Right":
                    SendMessage(handle, 0x112, (IntPtr)11, IntPtr.Zero); // WMSZ_RIGHT
                    break;
                case "BottomLeft":
                    SendMessage(handle, 0x112, (IntPtr)16, IntPtr.Zero); // WMSZ_BOTTOMLEFT
                    break;
                case "Bottom":
                    SendMessage(handle, 0x112, (IntPtr)15, IntPtr.Zero); // WMSZ_BOTTOM
                    break;
                case "BottomRight":
                    SendMessage(handle, 0x112, (IntPtr)17, IntPtr.Zero); // WMSZ_BOTTOMRIGHT
                    break;
            }
        }


        [DllImport("user32.dll")]
        private static extern IntPtr SendMessage(IntPtr hWnd, int msg, IntPtr wParam, IntPtr lParam);

        [DllImport("user32.dll")]
        private static extern bool ReleaseCapture();


        private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
                DragMove();
        }



        
    }
}