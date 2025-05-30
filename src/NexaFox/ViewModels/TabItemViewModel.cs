﻿using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;


namespace NexaFox.ViewModels
{
    public class TabItemViewModel : INotifyPropertyChanged
    {
        private string _header;
        public string Header
        {
            get => _header;
            set
            {
                _header = value;
                OnPropertyChanged();
            }
        }

        private TabContentViewModelBase _content;
        public TabContentViewModelBase Content
        {
            get => _content;
            set
            {
                _content = value;
                Header = value?.Title ?? "Nowa karta";
                OnPropertyChanged();
            }
        }

        public FrameworkElement View { get; set; }


        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)=>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}