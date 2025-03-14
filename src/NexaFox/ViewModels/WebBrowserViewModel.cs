using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using CommunityToolkit.Mvvm.Input;

namespace NexaFox.ViewModels
{
    public class WebBrowserViewModel 
    {
        private string _address = "https://google.com";
        public string Address
        {
            get => _address;
            set
            {
                _address = value;
                OnPropertyChanged();
            }
        }



        public ICommand NavigateCommand => new RelayCommand(Navigate);

        private void Navigate()
        {
            NavigateRequested?.Invoke(this, Address);
        }

        public event Action RequestGoBack;
        public event EventHandler<string> NavigateRequested;

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));

        public ICommand GoBackCommand { get; }
        public ICommand GoForwardCommand { get; }
        public ICommand RefreshCommand { get; }
        public ICommand MenuCommand { get; }

        private string _currentUrl;
        public string CurrentUrl
        {
            get => _currentUrl;

        }

        
    }
}
