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

    private void CancelScan()=>_cancellationTokenSource?.Cancel();
    
}




