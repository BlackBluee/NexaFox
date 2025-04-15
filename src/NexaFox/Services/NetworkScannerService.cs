using NexaFox.Models;
using System.Diagnostics;
using System.Net.Sockets;
using System.Net;
using NexaFox.Utilities;
using System.Net.NetworkInformation;

namespace NexaFox.Services;
public class NetworkScannerService
{
    public List<PortOption> GetAvailablePortOptions()
    {
        var options = new List<PortOption>();
        foreach (var port in PortServiceMap.PortServices)
        {
            options.Add(new PortOption
            {
                Port = port.Key,
                Description = $"{port.Key} - {port.Value}",
                IsSelected = port.Key == 80 || port.Key == 443 || port.Key == 22 
            });
        }
        return options;
    }

    public string GetServiceName(int port)
    {
        return PortServiceMap.PortServices.TryGetValue(port, out var service) ? service : "Unknown";
    }

    public async Task<List<PortEntry>> ScanNetworkAsync(
        int[] portsToScan,
        IProgress<int> progress,
        CancellationToken cancellationToken,
        Action<PortEntry> onPortFound)
    {
        var results = new List<PortEntry>();
        var localIPs = GetLocalNetworkIPs();

        int totalScans = localIPs.Count * portsToScan.Length;
        int completedScans = 0;

        await Task.Run(() =>
        {
            var options = new ParallelOptions
            {
                MaxDegreeOfParallelism = Environment.ProcessorCount * 2,
                CancellationToken = cancellationToken
            };

            try
            {
                Parallel.ForEach(localIPs, options, ip =>
                {
                    foreach (var port in portsToScan)
                    {
                        if (cancellationToken.IsCancellationRequested)
                            break;

                        var isOpen = CheckPort(ip, port, cancellationToken);
                        if (isOpen)
                        {
                            var entry = new PortEntry
                            {
                                IP = ip.ToString(),
                                Port = port,
                                Service = GetServiceName(port)
                            };

                            lock (results)
                            {
                                results.Add(entry);
                            }

                            onPortFound?.Invoke(entry);
                        }
                        int newProgress = (int)((double)Interlocked.Increment(ref completedScans) / totalScans * 100);
                        progress?.Report(newProgress);
                    }
                });
            }
            catch (OperationCanceledException)
            {
                Debug.WriteLine("Skanowanie zostało anulowane przez użytkownika");
            }
        }, cancellationToken);

        return results;
    }

    private bool CheckPort(IPAddress ip, int port, CancellationToken cancellationToken)
    {
        try
        {
            if (cancellationToken.IsCancellationRequested)
                return false;

            using var client = new TcpClient();
            client.Client.ReceiveTimeout = 3000;
            client.Client.SendTimeout = 3000;

            var result = client.BeginConnect(ip, port, null, null);
            var success = result.AsyncWaitHandle.WaitOne(TimeSpan.FromSeconds(3));

            if (cancellationToken.IsCancellationRequested)
                return false;

            if (success)
            {
                try
                {
                    client.EndConnect(result);
                    if (client.Connected)
                    {
                        Debug.WriteLine($"Znaleziono otwarty port: {ip}:{port}");
                        return true;
                    }
                }
                catch (SocketException)
                {

                }
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Błąd: {ip}:{port} - {ex.Message}");
        }

        return false;
    }

    public List<IPAddress> GetLocalNetworkIPs()
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

                    Debug.WriteLine($"Znaleziono lokalny adres IP: {ipInfo.Address}, maska: {ipInfo.IPv4Mask}");
                    if (!ips.Contains(ipInfo.Address))
                    {
                        ips.Add(ipInfo.Address);
                    }

                    uint networkAddress = NetworkCalculator.CalculateNetworkAddress(ipInfo.Address, ipInfo.IPv4Mask);
                    uint broadcastAddress = NetworkCalculator.CalculateBroadcastAddress(ipInfo.Address, ipInfo.IPv4Mask);
                    uint networkSize = broadcastAddress - networkAddress - 1;

                    Debug.WriteLine($"Adres sieci: {new IPAddress(BitConverter.GetBytes(networkAddress).Reverse().ToArray())}");
                    Debug.WriteLine($"Adres broadcast: {new IPAddress(BitConverter.GetBytes(broadcastAddress).Reverse().ToArray())}");
                    Debug.WriteLine($"Rozmiar sieci: {networkSize} hostów");

                    uint maxHostsToScan = networkSize <= 254 ? networkSize : 254;

                    double step = networkSize > maxHostsToScan ? (double)networkSize / maxHostsToScan : 1;

                    for (double i = 1; i <= networkSize && ips.Count < networkSize + 1; i += step)
                    {
                        uint hostAddress = networkAddress + (uint)Math.Round(i);

                        byte[] addressBytes = BitConverter.GetBytes(hostAddress);
                        if (BitConverter.IsLittleEndian)
                            Array.Reverse(addressBytes);

                        var ipAddress = new IPAddress(addressBytes);

                        if (!ips.Contains(ipAddress))
                        {
                            ips.Add(ipAddress);
                            Debug.WriteLine($"Dodano adres do skanowania: {ipAddress}");
                        }
                    }
                }
            }
            if (!ips.Contains(IPAddress.Loopback))
            {
                ips.Add(IPAddress.Loopback);
                Debug.WriteLine($"Dodano localhost: {IPAddress.Loopback}");
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Błąd podczas wykrywania adresów IP: {ex.Message}");
            Debug.WriteLine("Używam zapasowej listy adresów");

            ips.Add(IPAddress.Loopback);

            string[] commonNetworks = {
                "192.168.0.", "192.168.1.", "192.168.88.",
                "10.0.0.", "10.0.1.",
                "172.16.0.", "172.17.0."
            };

            foreach (var network in commonNetworks)
            {
                for (int i = 1; i <= 10; i++)
                {
                    ips.Add(IPAddress.Parse($"{network}{i}"));
                }

                int[] higherIPs = { 50, 100, 150, 200, 254 };
                foreach (var ipEnd in higherIPs)
                {
                    ips.Add(IPAddress.Parse($"{network}{ipEnd}"));
                }
            }
        }
        var uniqueIps = ips.Distinct().OrderBy(ip =>
        {
            byte[] bytes = ip.GetAddressBytes();
            return (bytes[0] << 24) | (bytes[1] << 16) | (bytes[2] << 8) | bytes[3];
        }).ToList();

        Debug.WriteLine($"Łącznie znaleziono {uniqueIps.Count} adresów do skanowania.");
        return uniqueIps;
    }
}
