using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NexaFox.ViewModels
{
    class PortMonitorViewModel : TabContentViewModelBase
    {
        public ObservableCollection<PortEntry> PortEntries { get; } = new ObservableCollection<PortEntry>();
        public PortMonitorViewModel()
        {
            Title = "Port Monitor";
        }

        public async Task StartNetworkScan()
        {
            PortEntries.Clear();

            await Task.Run(() =>
            {
                // Przykładowe adresy IP i porty 
                var fakeResults = new List<PortEntry>
            {
                new PortEntry { IP = "192.168.1.1", Port = 80, Service = "HTTP" },
                new PortEntry { IP = "192.168.1.2", Port = 22, Service = "SSH" },
                new PortEntry { IP = "192.168.1.3", Port = 21, Service = "FTP" }
            };

                Application.Current.Dispatcher.Invoke(() =>
                {
                    foreach (var entry in fakeResults)
                    {
                        PortEntries.Add(entry);
                    }
                });
            });
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged(string propertyName) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}



