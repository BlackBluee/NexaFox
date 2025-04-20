using NexaFox.Models;
using System.Collections.ObjectModel;
using System.Windows;
using CommunityToolkit.Mvvm.Input;
using System.Diagnostics;
using System.Windows.Data;
using System.Windows.Input;
using NexaFox.Services;

namespace NexaFox.ViewModels;
public class PortMonitorViewModel : TabContentViewModelBase
{
    private readonly NetworkScannerService _scannerService;
    private readonly object _lock = new object();
    private bool _isScanning;
    private CancellationTokenSource? _cancellationTokenSource;
    private int _progress;
    private MainViewModel _mainViewModel;

    public ObservableCollection<PortEntry> PortEntries { get; } = new ObservableCollection<PortEntry>();
    public ObservableCollection<Models.PortOption> AvailablePorts { get; } = new ObservableCollection<Models.PortOption>();

    private RelayCommand? _scanCommand;
    public ICommand ScanCommand => _scanCommand ??= new RelayCommand(
        async () => await StartNetworkScan(),
        () => !IsScanning
    );

    private RelayCommand? _cancelCommand;
    public ICommand CancelCommand => _cancelCommand ??= new RelayCommand(
        CancelScan,
        () => IsScanning
    );

    private RelayCommand<PortEntry>? _connectPortCommand;
    public ICommand ConnectPortCommand => _connectPortCommand ??= new RelayCommand<PortEntry>(
        ConnectToPort,
        entry => entry != null && entry.Port > 0 && !string.IsNullOrEmpty(entry.IP)
    );

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
                _scanCommand?.NotifyCanExecuteChanged();
                _cancelCommand?.NotifyCanExecuteChanged();
            }
        }
    }

    public PortMonitorViewModel(NetworkScannerService? scannerService = null)
    {
        Title = "Port Monitor";
        _scannerService = scannerService ?? new NetworkScannerService();
        BindingOperations.EnableCollectionSynchronization(PortEntries, _lock);
        InitializeAvailablePorts();

        _mainViewModel = (Application.Current.MainWindow.DataContext as MainViewModel)
            ?? throw new InvalidOperationException("MainViewModel not found");
    }

    private void InitializeAvailablePorts()
    {
        AvailablePorts.Clear();
        var portOptions = _scannerService.GetAvailablePortOptions();
        foreach (var option in portOptions)
        {
            AvailablePorts.Add(option);
        }
    }

    private void ConnectToPort(PortEntry portEntry)
    {
        if (portEntry == null || string.IsNullOrEmpty(portEntry.IP) ||
            portEntry.IP == "Nie znaleziono" || portEntry.IP == "Błąd" ||
            portEntry.IP == "Anulowano" || portEntry.IP == "Skanowanie...")
        {
            return;
        }

        try
        {
            switch (portEntry.Port)
            {
                case 80:
                case 443:
                case 8080:
                case 8443:
                    OpenWebBrowser(portEntry.IP, portEntry.Port);
                    break;

                case 21:
                case 2121:
                    OpenFTP(portEntry.IP, portEntry.Port);
                    break;

                case 22:
                case 222:
                case 2222:
                default:
                    OpenSSH(portEntry.IP, portEntry.Port);
                    break;
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Błąd podczas otwierania połączenia: {ex.Message}",
                "Błąd połączenia", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void OpenWebBrowser(string ip, int port)
    {
        var protocol = (port == 443 || port == 8443) ? "https" : "http";
        var uri = $"{protocol}://{ip}";

        if ((port != 80 && port != 443) ||
            (port == 80 && protocol == "https") ||
            (port == 443 && protocol == "http"))
        {
            uri += $":{port}";
        }

        var browserView = new Views.Pages.WebBrowser();
        var viewModel = new WebBrowserViewModel();
        viewModel.Address = uri;

        var newTab = new TabItemViewModel
        {
            Content = viewModel,
            View = browserView
        };

        browserView.DataContext = viewModel;
        _mainViewModel.Tabs.Add(newTab);
        _mainViewModel.SelectedTab = newTab;
    }

    private void OpenSSH(string ip, int port)
    {
        var sshView = new Views.Pages.ClientSSH();
        var viewModel = new SSHViewModel();

        viewModel.Host = ip;
        viewModel.Port = port.ToString();

        var newTab = new TabItemViewModel
        {
            Content = viewModel,
            View = sshView
        };

        sshView.DataContext = viewModel;
        _mainViewModel.Tabs.Add(newTab);
        _mainViewModel.SelectedTab = newTab;
    }

    private void OpenFTP(string ip, int port)
    {
        var ftpView = new Views.Pages.ClientFTP();
        var viewModel = new FTPViewModel();

        // Ustawić właściwości FTPViewModel (jeśli są dostępne)
        // viewModel.Host = ip;
        // viewModel.Port = port.ToString();

        var newTab = new TabItemViewModel
        {
            Content = viewModel,
            View = ftpView
        };

        ftpView.DataContext = viewModel;
        _mainViewModel.Tabs.Add(newTab);
        _mainViewModel.SelectedTab = newTab;
    }

    public async Task StartNetworkScan()
    {
        if (IsScanning) return;

        try
        {
            _cancellationTokenSource = new CancellationTokenSource();
            var token = _cancellationTokenSource.Token;

            IsScanning = true;
            PortEntries.Clear();

            var portsToScan = AvailablePorts
                .Where(p => p.IsSelected)
                .Select(p => p.Port)
                .ToArray();

            if (portsToScan.Length == 0)
            {
                MessageBox.Show("Proszę wybrać co najmniej jeden port do skanowania.",
                    "Brak wybranych portów", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            Progress = 0;

            Application.Current.Dispatcher.Invoke(() =>
            {
                PortEntries.Add(new PortEntry
                {
                    IP = "Skanowanie...",
                    Port = 0,
                    Service = "Sprawdzanie adresów IP"
                });
            });

            var progress = new Progress<int>(p => Progress = p);

            await _scannerService.ScanNetworkAsync(
                portsToScan,
                progress,
                token,
                portEntry =>
                {
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        PortEntries.Add(portEntry);
                    });
                });

            Application.Current.Dispatcher.Invoke(() =>
            {
                var infoEntry = PortEntries.FirstOrDefault(p => p.IP == "Skanowanie...");
                if (infoEntry != null)
                {
                    PortEntries.Remove(infoEntry);
                }

                if (token.IsCancellationRequested)
                {
                    PortEntries.Add(new PortEntry
                    {
                        IP = "Anulowano",
                        Port = 0,
                        Service = "Skanowanie zostało przerwane"
                    });
                }
                else if (PortEntries.Count == 0)
                {
                    PortEntries.Add(new PortEntry
                    {
                        IP = "Nie znaleziono",
                        Port = 0,
                        Service = "Brak otwartych portów"
                    });
                }
            });
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Błąd podczas skanowania: {ex.Message}");
            MessageBox.Show($"Wystąpił błąd podczas skanowania sieci: {ex.Message}",
                "Błąd skanowania", MessageBoxButton.OK, MessageBoxImage.Error);

            Application.Current.Dispatcher.Invoke(() =>
            {
                PortEntries.Add(new PortEntry
                {
                    IP = "Błąd",
                    Port = 0,
                    Service = ex.Message
                });
            });
        }
        finally
        {
            IsScanning = false;
            _cancellationTokenSource?.Dispose();
            _cancellationTokenSource = null;
        }
    }

    private void CancelScan() => _cancellationTokenSource?.Cancel();
}




