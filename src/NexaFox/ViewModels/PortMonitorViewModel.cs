using NexaFox.Models;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
namespace NexaFox.ViewModels;
using NexaFox.Utilities;
class PortMonitorViewModel : TabContentViewModelBase, INotifyPropertyChanged
{
    public ObservableCollection<PortEntry> PortEntries { get; } = new ObservableCollection<PortEntry>();



    public PortMonitorViewModel()
    {
        Title = "Port Monitor";
    }

    public async Task StartNetworkScan()
    {
        PortEntries.Clear();

        var localIPs = GetLocalNetworkIPs();
        var portsToScan = new int[] { 21, 22, 80, 443, 8080 };

        await Task.Run(async () =>
        {
            var tasks = new List<Task>();

            foreach (var ip in localIPs)
            {
                foreach (var port in portsToScan)
                {
                    tasks.Add(CheckPortAsync(ip, port));
                }
            }

            await Task.WhenAll(tasks);
        });
    }

    private async Task CheckPortAsync(IPAddress ip, int port)
    {
        try
        {
            using var client = new TcpClient();
            var cts = new CancellationTokenSource(TimeSpan.FromMilliseconds(500));

            await client.ConnectAsync(ip, port, cts.Token);

            if (client.Connected)
            {
                var service = GetServiceName(port);
                Application.Current.Dispatcher.Invoke(() =>
                {
                    PortEntries.Add(new PortEntry
                    {
                        IP = ip.ToString(),
                        Port = port,
                        Service = service
                    });
                });
            }
        }
        catch { }
    }

    private List<IPAddress> GetLocalNetworkIPs()
    {
        var ips = new List<IPAddress>();

        foreach (var ni in NetworkInterface.GetAllNetworkInterfaces())
        {
            if (ni.OperationalStatus != OperationalStatus.Up) continue;

            foreach (var ipInfo in ni.GetIPProperties().UnicastAddresses)
            {
                if (ipInfo.Address.AddressFamily != AddressFamily.InterNetwork) continue;
                if (ipInfo.IPv4Mask == null) continue;

                uint network = NetworkCalculator.CalculateNetworkAddress(ipInfo.Address, ipInfo.IPv4Mask);
                uint broadcast = NetworkCalculator.CalculateBroadcastAddress(ipInfo.Address, ipInfo.IPv4Mask);

                for (uint i = 1; i <= broadcast - network; i++)
                {
                    uint currentAddress = network + i;
                    byte[] addressBytes = BitConverter.GetBytes(currentAddress);
                    if (BitConverter.IsLittleEndian) Array.Reverse(addressBytes);
                    ips.Add(new IPAddress(addressBytes));
                }

                return ips; 
            }
        }
        return ips;
    }

    

    private string GetServiceName(int port) => port switch
    {
        21 => "FTP",
        22 => "SSH",
        80 => "HTTP",
        443 => "HTTPS",
        8080 => "HTTP Alt",
        _ => "Unknown"
    };

    public event PropertyChangedEventHandler PropertyChanged;
    protected virtual void OnPropertyChanged(string propertyName) =>
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
}

