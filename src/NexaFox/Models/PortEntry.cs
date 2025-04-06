using System.ComponentModel;


namespace NexaFox.Models
{
    public class PortEntry : INotifyPropertyChanged
    {
        public string IP { get; set; }
        public int Port { get; set; }
        public string Service { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;
    }
}

