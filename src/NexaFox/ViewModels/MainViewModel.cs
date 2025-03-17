using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using CommunityToolkit.Mvvm.Input;


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
            AddNewTab(); // Dodaj początkową kartę
        }

        private void AddNewTab()
        {
            var newTab = new TabItemViewModel
            {
                Content = new WebBrowserViewModel()
                
            };
            Tabs.Add(newTab);
            SelectedTab = newTab;
        }

        private void AddNewClientFTP()
        {
            var newTab = new TabItemViewModel
            {
                Content = new FTPViewModel()
            };
            Tabs.Add(newTab);
            SelectedTab = newTab;
        }

        private void AddNewClientSSH()
        {
            var newTab = new TabItemViewModel
            {
                Content = new SSHViewModel()
            };
            Tabs.Add(newTab);
            SelectedTab = newTab;
        }

        private void AddNewPortMonitor()
        {
            var newTab = new TabItemViewModel
            {
                Content = new PortMonitorViewModel()
            };
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