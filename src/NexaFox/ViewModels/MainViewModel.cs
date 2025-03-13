using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.AspNetCore.Components.Web;
using NexaFox.Services.Interfaces;
using System.ComponentModel;
using System.Net;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;

namespace NexaFox.ViewModels
{
    public partial class MainViewModel : ObservableObject
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