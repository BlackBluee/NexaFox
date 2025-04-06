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
                    if (!ips.Contains(ipInfo.Address))
                    {
                        ips.Add(ipInfo.Address);
                        Debug.WriteLine($"Dodano własny adres IP: {ipInfo.Address}");
                    }

                    uint networkAddress = NetworkCalculator.CalculateNetworkAddress(ipInfo.Address, ipInfo.IPv4Mask);
                    uint broadcastAddress = NetworkCalculator.CalculateBroadcastAddress(ipInfo.Address, ipInfo.IPv4Mask);

                    byte[] netAddressBytes = BitConverter.GetBytes(networkAddress);
                    if (BitConverter.IsLittleEndian) Array.Reverse(netAddressBytes);
                    byte[] broadAddressBytes = BitConverter.GetBytes(broadcastAddress);
                    if (BitConverter.IsLittleEndian) Array.Reverse(broadAddressBytes);

                    var netIP = new IPAddress(netAddressBytes);
                    var broadIP = new IPAddress(broadAddressBytes);

                    uint networkSize = broadcastAddress - networkAddress - 1;

                    uint hostsToScan;
                    if (networkSize <= 10)
                    {
                        hostsToScan = networkSize;
                    }
                    else if (networkSize <= 254)
                    {
                        hostsToScan = Math.Min(100, networkSize);
                    }
                    else
                    {
                        hostsToScan = Math.Min(150, networkSize);
                    }

                    if (networkSize <= 254)
                    {
                        double step = (double)networkSize / hostsToScan;
                        for (double i = 1; i <= networkSize && ips.Count < networkSize; i += step)
                        {
                            uint hostAddress = networkAddress + (uint)Math.Round(i);
                            byte[] addressBytes = BitConverter.GetBytes(hostAddress);
                            if (BitConverter.IsLittleEndian) Array.Reverse(addressBytes);
                            var ipAddress = new IPAddress(addressBytes);

                            if (!ips.Contains(ipAddress))
                            {
                                ips.Add(ipAddress);
                            }
                        }
                    }
                    else
                    {
                        uint routerAddress = networkAddress + 1;
                        byte[] routerBytes = BitConverter.GetBytes(routerAddress);
                        if (BitConverter.IsLittleEndian) Array.Reverse(routerBytes);
                        var routerIP = new IPAddress(routerBytes);

                        if (!ips.Contains(routerIP))
                        {
                            ips.Add(routerIP);
                        }

                        uint[] popularHosts = { 10, 20, 30, 50, 100, 150, 200, 250, 254 };
                        foreach (uint host in popularHosts)
                        {
                            if (host < networkSize)
                            {
                                uint serverAddress = networkAddress + host;
                                byte[] serverBytes = BitConverter.GetBytes(serverAddress);
                                if (BitConverter.IsLittleEndian) Array.Reverse(serverBytes);
                                var serverIP = new IPAddress(serverBytes);

                                if (!ips.Contains(serverIP))
                                {
                                    ips.Add(serverIP);
                                }
                            }
                        }

                        if (hostsToScan > popularHosts.Length)
                        {
                            double step = (double)networkSize / (hostsToScan - popularHosts.Length);

                            for (double i = 2; i < networkSize && ips.Count < hostsToScan + 1; i += step)
                            {
                                uint hostAddress = networkAddress + (uint)Math.Round(i);
                                byte[] addressBytes = BitConverter.GetBytes(hostAddress);
                                if (BitConverter.IsLittleEndian) Array.Reverse(addressBytes);
                                var ipAddress = new IPAddress(addressBytes);

                                if (!ips.Contains(ipAddress))
                                {
                                    ips.Add(ipAddress);
                                }
                            }
                        }
                    }
                }
            }

            if (!ips.Contains(IPAddress.Loopback))
            {
                ips.Add(IPAddress.Loopback);
            }
        }
        catch (Exception ex)
        {
            ips.Add(IPAddress.Loopback);
            try
            {
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
            catch
            {
            }
        }

        var uniqueIps = ips.Distinct().OrderBy(ip =>
        {
            byte[] bytes = ip.GetAddressBytes();
            return (bytes[0] << 24) | (bytes[1] << 16) | (bytes[2] << 8) | bytes[3];
        }).ToList();
        return uniqueIps;
    }
}
