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
            InitializeWebView();
            ((MainViewModel)DataContext).NavigateRequested += OnNavigateRequested;
        }
        private async void InitializeWebView()
        {
            await webView.EnsureCoreWebView2Async(null);
            webView.Source = new Uri("https://www.google.com");
            var windowChrome = new WindowChrome
            {
                ResizeBorderThickness = new Thickness(8), // Dopasuj do grubości prostokątów
                CaptionHeight = 0,
                CornerRadius = new CornerRadius(8),
                GlassFrameThickness = new Thickness(0)
            };
            WindowChrome.SetWindowChrome(this, windowChrome);
        }

        private void OnNavigateRequested(object sender, string url)
        {
            if (Uri.IsWellFormedUriString(url, UriKind.Absolute))
            {
                webView.CoreWebView2?.Navigate(url);
            }
        }

        

        private void WebView_NavigationCompleted(object sender, CoreWebView2NavigationCompletedEventArgs e)
        {
            if (e.IsSuccess && webView.CoreWebView2 != null)
            {
                string currentUrl = webView.CoreWebView2.Source;
                ((MainViewModel)DataContext).Address = currentUrl;
            }
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

        // Interop do obsługi przeciągania
        [DllImport("user32.dll")]
        private static extern IntPtr SendMessage(IntPtr hWnd, int msg, IntPtr wParam, IntPtr lParam);

        [DllImport("user32.dll")]
        private static extern bool ReleaseCapture();


        private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
                DragMove();
        }



        private void Back_Click(object sender, RoutedEventArgs e)
        {
            if (webView.CanGoBack)
                webView.GoBack();
            string currentUrl = webView.CoreWebView2.Source;
            ((MainViewModel)DataContext).Address = currentUrl;
        }

        private void Forward_Click(object sender, RoutedEventArgs e)
        {
            if (webView.CanGoForward)
                webView.GoForward();
            string currentUrl = webView.CoreWebView2.Source;
            ((MainViewModel)DataContext).Address = currentUrl;
        }

        private void Refresh_Click(object sender, RoutedEventArgs e)
        {
            webView.Reload();
        }

        private void Menu_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Menu clicked!");
        }

        private void Options_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Options clicked!");
        }
    }
}