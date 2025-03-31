using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using CommunityToolkit.Mvvm.Input;

namespace NexaFox.ViewModels
{
    public class WebBrowserViewModel : TabContentViewModelBase
    {
        
        public WebBrowserViewModel()
        {
            Title = "Web Browser";
            CurrentUrl = "about:blank"; 
        }

            private string _address;
            private bool _isNavigationInProgress;

            public string Address
            {
                get => _address;
                set
                {
                    if (_address != value)
                    {
                        _address = value;
                        OnPropertyChanged();

                        if (!_isNavigationInProgress)
                        {
                            NavigateRequested?.Invoke(this, value);
                        }
                    }
                }
            }

            public ICommand NavigateCommand => new RelayCommand(Navigate);
            public ICommand GoBackCommand => new RelayCommand(GoBack);
            public ICommand GoForwardCommand => new RelayCommand(GoForward);

            public ICommand RefreshCommand => new RelayCommand(Refresh);
            private void Navigate() => NavigateRequested?.Invoke(this, Address);
            public void GoBack() => RequestGoBack?.Invoke();
            public void GoForward() => RequestGoForward?.Invoke();
            public void Refresh() => RequestRefresh?.Invoke();


            public event Action RequestGoBack;
            public event Action RequestGoForward;
            public event Action RequestRefresh;
            public event EventHandler<string> NavigateRequested;

            public event PropertyChangedEventHandler? PropertyChanged;
            protected void OnPropertyChanged([CallerMemberName] string name = null) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));


        private string _currentUrl = "about:blank"; 
        public string CurrentUrl
        {
            get => _currentUrl ?? "about:blank"; 
            set
            {
                if (_currentUrl != value)
                {
                    _currentUrl = value;
                    OnPropertyChanged();
                }
            }
        }

            
            private string _lastNavigatedUrl;
        public string LastNavigatedUrl
        {
            get => _lastNavigatedUrl;
            set
            {
                if (_lastNavigatedUrl != value)
                {
                    _lastNavigatedUrl = value;
                    OnPropertyChanged();
                }
            }
        }

        public void SetAddressWithoutNavigation(string url)
        {
            _isNavigationInProgress = true;
            Address = url;
            _isNavigationInProgress = false;
        }
        
    }
}
