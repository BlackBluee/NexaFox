using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Shell;
using NexaFox.ViewModels;


namespace NexaFox.Views
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            InitializeResize();

            MainTabControl.SelectionChanged += MainTabControl_SelectionChanged;
        }

        private void MainTabControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                if (DataContext is MainViewModel viewModel)
                {
                    viewModel.HandleTabSelectionChanged(sender, e);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Błąd podczas przełączania zakładek: {ex.Message}");
            }
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