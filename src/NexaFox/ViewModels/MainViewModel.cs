using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Controls;
using System.Windows.Input;
using NexaFox.Commands; // Zmień na właściwy namespace

namespace NexaFox.ViewModels
{
    public class MainViewModel : INotifyPropertyChanged
    {
        public ObservableCollection<TabItemViewModel> Tabs { get; } = new ObservableCollection<TabItemViewModel>();
        private TabItemViewModel _selectedTab;

        public ICommand AddTabCommand { get; }
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
            CloseTabCommand = new RelayCommand<TabItemViewModel>(CloseTab);
            AddNewTab(); // Dodaj początkową kartę
        }

        private void AddNewTab()
        {
            var newTab = new TabItemViewModel
            {
                Header = $"Nowa karta {Tabs.Count + 1}",
                Content = new TextBlock { Text = $"Zawartość karty {Tabs.Count + 1}" }
            };
            Tabs.Add(newTab);
            SelectedTab = newTab;
        }

        private void CloseTab(TabItemViewModel tab)
        {
            Tabs.Remove(tab);
            if (Tabs.Count > 0) SelectedTab = Tabs.Last();
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    
}