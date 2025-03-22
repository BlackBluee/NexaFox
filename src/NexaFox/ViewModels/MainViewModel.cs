using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using CommunityToolkit.Mvvm.Input;
using NexaFox.Views.Pages;


namespace NexaFox.ViewModels
{
    public partial class MainViewModel : INotifyPropertyChanged
    {
        public ObservableCollection<TabItemViewModel> Tabs { get; } = new ObservableCollection<TabItemViewModel>();
        private TabItemViewModel _selectedTab;

        public ICommand AddTabCommand { get; }
        public ICommand AddFTPCommand { get; }
        public ICommand AddSSHCommand { get; }
        public ICommand AddMonitorCommand { get; }
        public ICommand CloseTabCommand { get; }

        public TabItemViewModel SelectedTab
        {
            get => _selectedTab;
            set
            {
                
                _selectedTab = value;
                OnPropertyChanged();

            }
        }

        public MainViewModel()
        {
            AddTabCommand = new RelayCommand(AddNewTab);
            AddFTPCommand = new RelayCommand(AddNewClientFTP);
            AddSSHCommand = new RelayCommand(AddNewClientSSH);
            AddMonitorCommand = new RelayCommand(AddNewPortMonitor);
            CloseTabCommand = new RelayCommand<TabItemViewModel>(CloseTab);
            AddNewTab(); 
        }

        // ...

        public void HandleTabSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                if (e.RemovedItems.Count > 0 && e.RemovedItems[0] is TabItemViewModel prevTab &&
                    prevTab.Content is WebBrowserViewModel prevViewModel)
                {
                    var webBrowser = FindWebBrowserForViewModel(prevViewModel);
                    if (webBrowser != null)
                    {
                        webBrowser.SaveState();

                        if (string.IsNullOrEmpty(prevViewModel.CurrentUrl))
                        {
                            prevViewModel.CurrentUrl = "about:blank";
                        }
                    }
                }

                if (SelectedTab?.Content is WebBrowserViewModel currentViewModel)
                {
                    if (string.IsNullOrEmpty(currentViewModel.CurrentUrl))
                    {
                        currentViewModel.CurrentUrl = "about:blank";
                    }
                    var webBrowser = FindWebBrowserForViewModel(currentViewModel);
                    if (webBrowser != null)
                    {
                        webBrowser.RestoreState();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Błąd podczas przełączania zakładek: {ex.Message}");
            }
        }

        private Views.Pages.WebBrowser FindWebBrowserForViewModel(WebBrowserViewModel viewModel)
        {
            if (Application.Current.MainWindow is Window mainWindow)
            {
                return FindWebBrowserInVisualTree(mainWindow, viewModel);
            }
            return null;
        }

        private Views.Pages.WebBrowser FindWebBrowserInVisualTree(DependencyObject parent, WebBrowserViewModel viewModel)
        {
            if (parent is Views.Pages.WebBrowser webBrowser && webBrowser.DataContext == viewModel)
            {
                return webBrowser;
            }

            int childCount = VisualTreeHelper.GetChildrenCount(parent);
            for (int i = 0; i < childCount; i++)
            {
                var child = VisualTreeHelper.GetChild(parent, i);
                var result = FindWebBrowserInVisualTree(child, viewModel);
                if (result != null)
                {
                    return result;
                }
            }

            return null;
        }

        private void AddNewTab()
        {
            var browserView = new NexaFox.Views.Pages.WebBrowser();
            var viewModel = new WebBrowserViewModel();

            var newTab = new TabItemViewModel
            {
                Content = viewModel,
                View = browserView 
            };

            browserView.DataContext = viewModel;
            Tabs.Add(newTab);
            SelectedTab = newTab;
        }

        private void AddNewClientFTP()
        {
            
           

                var ftpView = new NexaFox.Views.Pages.ClientFTP();
                var viewModel = new FTPViewModel();

                var newTab = new TabItemViewModel
                {
                    Content = viewModel,
                    View = ftpView
                };

                ftpView.DataContext = viewModel;
                Tabs.Add(newTab);
                SelectedTab = newTab;
            
        }

        private void AddNewClientSSH()
        {
            var sshView = new NexaFox.Views.Pages.ClientSSH();
            var viewModel = new SSHViewModel();

            var newTab = new TabItemViewModel
            {
                Content = viewModel,
                View = sshView
            };

            sshView.DataContext = viewModel;
            Tabs.Add(newTab);
            SelectedTab = newTab;
        }

        private void AddNewPortMonitor()
        {
            var monitorView = new NexaFox.Views.Pages.PortMonitor();
            var viewModel = new PortMonitorViewModel();

            var newTab = new TabItemViewModel
            {
                Content = viewModel,
                View = monitorView
            };

            monitorView.DataContext = viewModel;
            Tabs.Add(newTab);
            SelectedTab = newTab;
        }

        private void CloseTab(TabItemViewModel tab)
        {
            if (tab != null)
            {
                (tab.Content as IDisposable)?.Dispose();
                Tabs.Remove(tab);
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        [RelayCommand]
        private void Minimize()
        {
            if (Application.Current.MainWindow is Window window)
            {
                window.WindowState = WindowState.Minimized;
            }
        }

        [RelayCommand]
        private void Maximize()
        {
            if (Application.Current.MainWindow is Window window)
            {
                window.WindowState = window.WindowState == WindowState.Maximized
                    ? WindowState.Normal
                    : WindowState.Maximized;
            }
        }

        [RelayCommand]
        private void Close()
        {
            if (Application.Current.MainWindow is Window window)
            {
                window.Close();
            }
        }
    }

    
}