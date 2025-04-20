using System.ComponentModel;

namespace NexaFox.Models;

public class PortEntry : INotifyPropertyChanged
{
    private string _ip;
    private int _port;
    private string _service;

    public string IP
    {
        get => _ip;
        set
        {
            if (_ip != value)
            {
                _ip = value;
                OnPropertyChanged(nameof(IP));
            }
        }
    }

    public int Port
    {
        get => _port;
        set
        {
            if (_port != value)
            {
                _port = value;
                OnPropertyChanged(nameof(Port));
            }
        }
    }

    public string Service
    {
        get => _service;
        set
        {
            if (_service != value)
            {
                _service = value;
                OnPropertyChanged(nameof(Service));
            }
        }
    }

    public event PropertyChangedEventHandler PropertyChanged;

    protected void OnPropertyChanged(string propertyName)=>
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    
}


