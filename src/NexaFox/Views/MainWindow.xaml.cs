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






        

        private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
            {
                var handle = new WindowInteropHelper(this).Handle;
                ReleaseCapture();
                SendMessage(handle, 0xA1, (IntPtr)2, IntPtr.Zero);
            }
        }

        [DllImport("user32.dll")]
        private static extern int SendMessage(IntPtr hWnd, int msg, IntPtr wParam, IntPtr lParam);

        [DllImport("user32.dll")]
        private static extern bool ReleaseCapture();




    }
}