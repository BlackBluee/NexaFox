using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows;
using CommunityToolkit.Mvvm.Input;

namespace NexaFox.Models
{
    public class PortEntry : INotifyPropertyChanged
    {
        public string IP { get; set; }
        public int Port { get; set; }
        public string Service { get; set; }

        public ICommand ConnectCommand => new RelayCommand(Connect);

        private void Connect()
        {
            MessageBox.Show($"Łączenie z {IP}:{Port} ({Service})...");
        }

        
        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged(string propertyName) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

}
