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
            InitializeWebView();
            DataContextChanged += OnDataContextChanged;
        }

        private void OnDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (e.OldValue is WebBrowserViewModel oldVm)
            {
                oldVm.NavigateRequested -= OnNavigateRequested;
            }

            if (e.NewValue is WebBrowserViewModel newVm)
            {
                newVm.NavigateRequested += OnNavigateRequested;
            }
        }


        private async void InitializeWebView()
    {
        await webView.EnsureCoreWebView2Async(null);
        
        if (DataContext is WebBrowserViewModel vm)
        {
            webView.Source = new Uri(vm.Address ?? "https://www.google.com");
        }
        
        webView.NavigationCompleted += WebView_NavigationCompleted;
    }

        
        private void NavigateToViewModelUrl(WebBrowserViewModel vm)
    {
        if (!string.IsNullOrEmpty(vm.Address))
        {
            var uri = Uri.IsWellFormedUriString(vm.Address, UriKind.Absolute) 
                ? new Uri(vm.Address) 
                : new Uri("https://www.google.com/search?q=" + Uri.EscapeDataString(vm.Address));
            
            webView.CoreWebView2.Navigate(uri.ToString());
        }
    }

        private void OnNavigateRequested(object sender, string url)
        {
            if (DataContext is WebBrowserViewModel vm && vm == sender)
            {
                NavigateToViewModelUrl(vm);
            }
        }

        private void WebView_NavigationCompleted(object sender, CoreWebView2NavigationCompletedEventArgs e)
        {
            if (e.IsSuccess && DataContext is WebBrowserViewModel vm)
            {
                vm.SetAddressWithoutNavigation(webView.Source.ToString());
            }
        }

        private void OnGoBack() => webView.GoBack();
    private void OnGoForward() => webView.GoForward();
    private void OnRefresh() => webView.Reload();



    }
    
}
