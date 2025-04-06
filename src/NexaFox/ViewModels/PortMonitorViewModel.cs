using NexaFox.Models;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
namespace NexaFox.ViewModels;

using CommunityToolkit.Mvvm.Input;
using NexaFox.Utilities;
using System.Diagnostics;
using System.Windows.Data;
using System.Windows.Input;

public class PortMonitorViewModel : TabContentViewModelBase // Usunięto INotifyPropertyChanged
{
    private int _totalScans;
    private int _completedScans;
    private readonly object _lock = new object();
    private bool _isScanning;

    public ObservableCollection<PortEntry> PortEntries { get; } = new ObservableCollection<PortEntry>();

    private RelayCommand _scanCommand;
    public ICommand ScanCommand => _scanCommand ??= new RelayCommand(
        async () => await StartNetworkScan(),
        () => !IsScanning // Dodano warunek wykonania komendy
    );

    private int _progress;
    public int Progress
    {
        get => _progress;
        set
        {
            _progress = value;
            OnPropertyChanged();
        }
    }

    public bool IsScanning
    {
        get => _isScanning;
        private set
        {
            if (_isScanning != value)
            {
                _isScanning = value;
                OnPropertyChanged();
                _scanCommand.NotifyCanExecuteChanged();
            }
        }
    }

    public PortMonitorViewModel()
    {
        Title = "Port Monitor";
        BindingOperations.EnableCollectionSynchronization(PortEntries, _lock);
    }

    public async Task StartNetworkScan()
    {
        if (IsScanning) return;

        try
        {
            IsScanning = true;
            PortEntries.Clear();

            var localIPs = GetLocalNetworkIPs();
            var portsToScan = new[] { 21, 22, 80, 443, 8080 };

            _totalScans = localIPs.Count * portsToScan.Length;
            _completedScans = 0;
            Progress = 0;

            await Task.Run(() =>
            {
                var options = new ParallelOptions
                {
                    MaxDegreeOfParallelism = 20
                };

                Parallel.ForEach(localIPs, options, ip =>
                {
                    foreach (var port in portsToScan)
                    {
                        CheckPort(ip, port);
                        UpdateProgress();
                    }
                });
            });
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Błąd podczas skanowania: {ex.Message}");
            MessageBox.Show($"Wystąpił błąd podczas skanowania sieci: {ex.Message}",
                "Błąd skanowania", MessageBoxButton.OK, MessageBoxImage.Error);
        }
        finally
        {
            IsScanning = false;
        }
    }

    private void CheckPort(IPAddress ip, int port)
    {
        try
        {
            using var client = new TcpClient();
            var stopwatch = Stopwatch.StartNew();

            var result = client.BeginConnect(ip, port, null, null);
            var success = result.AsyncWaitHandle.WaitOne(1500);

            if (success && client.Connected)
            {
                client.EndConnect(result);
                Debug.WriteLine($"Znaleziono otwarty port: {ip}:{port} ({stopwatch.ElapsedMilliseconds}ms)");
                AddPortEntry(ip, port);
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Błąd: {ip}:{port} - {ex.Message}");
        }
    }

    private void AddPortEntry(IPAddress ip, int port)
    {
        lock (_lock)
        {
            PortEntries.Add(new PortEntry
            {
                IP = ip.ToString(),
                Port = port,
                Service = GetServiceName(port)
            });
        }
    }

    private void UpdateProgress()
    {
        var newProgress = (int)((double)Interlocked.Increment(ref _completedScans) / _totalScans * 100);

        // Aktualizuj tylko gdy zmiana >= 1%
        if (newProgress > Progress)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                Progress = newProgress;
            });
        }
    }

    private List<IPAddress> GetLocalNetworkIPs()
    {
        var ips = new List<IPAddress>();

        try
        {
            foreach (var ni in NetworkInterface.GetAllNetworkInterfaces())
            {
                if (ni.OperationalStatus != OperationalStatus.Up) continue;
                if (ni.NetworkInterfaceType == NetworkInterfaceType.Loopback) continue;

                foreach (var ipInfo in ni.GetIPProperties().UnicastAddresses)
                {
                    if (ipInfo.Address.AddressFamily != AddressFamily.InterNetwork) continue;
                    if (ipInfo.IPv4Mask == null) continue;

                    // Obliczanie adresu sieci i broadcast za pomocą NetworkCalculator
                    uint networkAddress = NetworkCalculator.CalculateNetworkAddress(ipInfo.Address, ipInfo.IPv4Mask);
                    uint broadcastAddress = NetworkCalculator.CalculateBroadcastAddress(ipInfo.Address, ipInfo.IPv4Mask);

                    // Generuj maksymalnie 20 adresów IP w sieci
                    uint maxIps = Math.Min(20, broadcastAddress - networkAddress - 1);

                    for (uint i = 1; i <= maxIps; i++)
                    {
                        uint hostAddress = networkAddress + i;
                        byte[] addressBytes = BitConverter.GetBytes(hostAddress);
                        if (BitConverter.IsLittleEndian) Array.Reverse(addressBytes);
                        ips.Add(new IPAddress(addressBytes));
                    }
                }
            }

            // Dodaj localhost dla pewności
            if (!ips.Contains(IPAddress.Loopback))
            {
                ips.Add(IPAddress.Loopback);
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Błąd podczas pobierania adresów sieciowych: {ex.Message}");
            // Dodaj localhost jako fallback
            ips.Add(IPAddress.Loopback);
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
}

