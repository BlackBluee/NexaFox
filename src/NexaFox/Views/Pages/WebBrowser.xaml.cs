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
        private string _cachedState;
        private Uri _lastNavigatedUri;
        private bool _isWebViewInitialized = false;
        private string _pendingNavigation = null;
        private void OnUnloaded(object sender, RoutedEventArgs e)
        {
            if (DataContext is WebBrowserViewModel viewModel)
            {
                SaveState();
                if (string.IsNullOrEmpty(viewModel.CurrentUrl))
                {
                    viewModel.CurrentUrl = "about:blank";
                }
            }
        }

        public WebBrowser()
        {
            InitializeComponent();
            InitializeWebView();
            DataContextChanged += OnDataContextChanged;
            Unloaded += OnUnloaded; 
        }
        
        private void OnDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (e.OldValue is WebBrowserViewModel oldVm)
            {
                oldVm.NavigateRequested -= OnNavigateRequested;
                oldVm.RequestGoBack -= OnGoBack;
                oldVm.RequestGoForward -= OnGoForward;
                oldVm.RequestRefresh -= OnRefresh;
            }

            if (e.NewValue is WebBrowserViewModel newVm)
            {
                newVm.NavigateRequested += OnNavigateRequested;
                newVm.RequestGoBack += OnGoBack;
                newVm.RequestGoForward += OnGoForward;
                newVm.RequestRefresh += OnRefresh;

                if (!string.IsNullOrEmpty(newVm.LastNavigatedUrl) && _isWebViewInitialized)
                {
                    NavigateToUrl(newVm.LastNavigatedUrl);
                }
            }
        }

        private async void InitializeWebView()
        {
            try
            {
                await webView.EnsureCoreWebView2Async(null);
                _isWebViewInitialized = true;

                webView.NavigationCompleted += WebView_NavigationCompleted;
                webView.CoreWebView2InitializationCompleted += WebView_CoreWebView2InitializationCompleted;

                if (_pendingNavigation != null && DataContext is WebBrowserViewModel vm)
                {
                    NavigateToUrl(_pendingNavigation);
                    _pendingNavigation = null;
                }
                else if (DataContext is WebBrowserViewModel viewModel)
                {
                    if (!string.IsNullOrEmpty(viewModel.LastNavigatedUrl))
                    {
                        NavigateToUrl(viewModel.LastNavigatedUrl);
                    }
                    else if (!string.IsNullOrEmpty(viewModel.Address))
                    {
                        NavigateToUrl(viewModel.Address);
                    }
                    else
                    {
                        NavigateToUrl("https://www.google.com");
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Błąd inicjalizacji WebView2: {ex.Message}");
            }
        }

        private void WebView_CoreWebView2InitializationCompleted(object sender, CoreWebView2InitializationCompletedEventArgs e)
        {
            if (e.IsSuccess)
            {
                if (DataContext is WebBrowserViewModel vm && !string.IsNullOrEmpty(vm.LastNavigatedUrl))
                {
                    NavigateToUrl(vm.LastNavigatedUrl);
                }
            }
            else
            {
                MessageBox.Show("Nie udało się zainicjalizować WebView2.");
            }
        }

        private void NavigateToViewModelUrl(WebBrowserViewModel vm)
        {
            if (!string.IsNullOrEmpty(vm.Address))
            {
                var url = vm.Address;
                NavigateToUrl(url);
            }
        }

        private void NavigateToUrl(string url)
        {
            if (string.IsNullOrEmpty(url))
                return;

            try
            {
                var uri = Uri.IsWellFormedUriString(url, UriKind.Absolute)
                    ? new Uri(url)
                    : new Uri("https://www.google.com/search?q=" + Uri.EscapeDataString(url));

                if (!_isWebViewInitialized || webView.CoreWebView2 == null)
                {
                    _pendingNavigation = uri.ToString();
                    return;
                }

                webView.CoreWebView2.Navigate(uri.ToString());
                _lastNavigatedUri = uri;

                if (DataContext is WebBrowserViewModel vm)
                {
                    vm.LastNavigatedUrl = uri.ToString();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Błąd podczas nawigacji: {ex.Message}");
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
                var currentUrl = webView.Source.ToString();
                vm.SetAddressWithoutNavigation(currentUrl);
                vm.LastNavigatedUrl = currentUrl;

                SaveState();
            }
        }

        private void OnGoBack()
        {
            if (_isWebViewInitialized && webView.CoreWebView2 != null && webView.CoreWebView2.CanGoBack)
            {
                webView.GoBack();
            }
        }

        private void OnGoForward()
        {
            if (_isWebViewInitialized && webView.CoreWebView2 != null && webView.CoreWebView2.CanGoForward)
            {
                webView.GoForward();
            }
        }

        private void OnRefresh()
        {
            if (_isWebViewInitialized && webView.CoreWebView2 != null)
            {
                webView.Reload();
            }
        }

        public void SaveState()
        {
            if (webView != null && _isWebViewInitialized && webView.CoreWebView2 != null)
            {
                if (DataContext is WebBrowserViewModel viewModel)
                {
                    string url = webView.Source?.ToString() ?? "about:blank";
                    viewModel.LastNavigatedUrl = url;
                    viewModel.CurrentUrl = url; 
                    _lastNavigatedUri = webView.Source ?? new Uri("about:blank");
                }
            }
        }

        public void RestoreState()
        {
            if (webView != null && _isWebViewInitialized && webView.CoreWebView2 != null)
            {
                if (DataContext is WebBrowserViewModel viewModel)
                {
                    string url = !string.IsNullOrEmpty(viewModel.LastNavigatedUrl)
                        ? viewModel.LastNavigatedUrl
                        : "about:blank";
                    NavigateToUrl(url);
                    viewModel.CurrentUrl = url;
                }
            }
        }
    }
    
}
