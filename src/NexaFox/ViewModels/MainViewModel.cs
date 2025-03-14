using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.AspNetCore.Components.Web;
using NexaFox.Services.Interfaces;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Net;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;
using NexaFox.Views.Pages;

namespace NexaFox.ViewModels
{
    public partial class MainViewModel : ObservableObject
    {
        [ObservableProperty]
        private TabItemViewModel? _selectedTab;

        public ObservableCollection<TabItemViewModel> Tabs { get; } = new();

        public MainViewModel()
        {
            AddInitialTab();
        }

        private void AddInitialTab()
        {
            var newTab = CreateNewTab();
            Tabs.Add(newTab);
            SelectedTab = newTab;
        }

        [RelayCommand]
        private void AddNewTab()
        {
            var newTab = CreateNewTab();
            Tabs.Add(newTab);
            SelectedTab = newTab;
        }

        private TabItemViewModel CreateNewTab()
        {
            return new TabItemViewModel
            {
                Content = new WebBrowser()
            };
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