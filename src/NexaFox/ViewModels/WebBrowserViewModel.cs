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


        private string _address = "https://google.com";
        public string Address
        {
            get => _address;
            set
            {
                Console.WriteLine($"Zmiana adresu: {_address} -> {value}");
                _address = value;
                OnPropertyChanged();
            }
        }

        public ICommand NavigateCommand => new RelayCommand(Navigate);
        public ICommand GoBackCommand => new RelayCommand(GoBack);
        public ICommand GoForwardCommand => new RelayCommand(GoForward);

        public ICommand RefreshCommand => new RelayCommand(Refresh);



        private void Navigate()
        {
            Console.WriteLine($"Navigating to: {Address}");
            NavigateRequested?.Invoke(this, Address);
        }

        private void GoBack()
        {
            RequestGoBack?.Invoke();
        }

        private void GoForward()
        {
            RequestGoForward?.Invoke();
        }
        private void Refresh()
        {
            RequestRefresh?.Invoke();
        }


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
