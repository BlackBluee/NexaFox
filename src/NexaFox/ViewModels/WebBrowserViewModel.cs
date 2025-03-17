using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace NexaFox.ViewModels
{
    public class WebBrowserViewModel : TabContentViewModelBase
    {
        public WebBrowserViewModel()
        {
            Title = "Web Browser";
            
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

        private string _cachedAddress;

        public void Activate()
        {
            if (Address != _cachedAddress)
            {
                NavigateRequested?.Invoke(this, Address);
            }
        }

        public void Deactivate()
        {
            _cachedAddress = Address;
        }

        public ICommand NavigateCommand => new RelayCommand(Navigate);
        public ICommand GoBackCommand => new RelayCommand(GoBack);
        public ICommand GoForwardCommand => new RelayCommand(GoForward);

        public ICommand RefreshCommand => new RelayCommand(Refresh);

        public void SetAddressWithoutNavigation(string url)
    {
        _isNavigationInProgress = true;
        Address = url;
        _isNavigationInProgress = false;
    }

        private void Navigate()
        {
            Console.WriteLine($"Navigating to: {Address}");
            NavigateRequested?.Invoke(this, Address);
        }

        public void GoBack() => RequestGoBack?.Invoke();
        public void GoForward() => RequestGoForward?.Invoke();
        public void Refresh() => RequestRefresh?.Invoke();


        public event Action RequestGoBack;
        public event Action RequestGoForward;
        public event Action RequestRefresh;
        public event EventHandler<string> NavigateRequested;

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));


        private string _currentUrl;
        public string CurrentUrl
        {
            get => _currentUrl;

        }

        
    }
}
