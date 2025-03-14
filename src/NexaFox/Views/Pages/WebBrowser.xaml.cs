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
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Shell;
using Microsoft.Web.WebView2.Core;
using NexaFox.ViewModels;

namespace NexaFox.Views.Pages
{
    public partial class WebBrowser : UserControl
    {
        private readonly WebBrowserViewModel _viewModel;
        public WebBrowser()
        {
            InitializeComponent();
            _viewModel = new WebBrowserViewModel();
            DataContext = _viewModel;


            _viewModel.NavigateRequested += OnNavigateRequested;
            InitializeWebView();
            
        }
        private async void InitializeWebView()
        {
            await webView.EnsureCoreWebView2Async(null);
            webView.Source = new Uri("https://www.google.com");
            
            
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
                ((WebBrowserViewModel)DataContext).Address = currentUrl;
            }
        }
        private void Back_Click(object sender, RoutedEventArgs e)
        {
            if (webView.CanGoBack)
                webView.GoBack();
            string currentUrl = webView.CoreWebView2.Source;
            ((WebBrowserViewModel)DataContext).Address = currentUrl;
        }

        private void Forward_Click(object sender, RoutedEventArgs e)
        {
            if (webView.CanGoForward)
                webView.GoForward();
            string currentUrl = webView.CoreWebView2.Source;
            ((WebBrowserViewModel)DataContext).Address = currentUrl;
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
