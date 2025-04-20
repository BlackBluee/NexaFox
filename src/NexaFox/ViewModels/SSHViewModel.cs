using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using CommunityToolkit.Mvvm.Input;
using Renci.SshNet;

namespace NexaFox.ViewModels
{
    public class SSHViewModel : TabContentViewModelBase, IDisposable
    {
        private SshClient? _sshClient;
        private ShellStream? _shellStream;
        private bool _isConnected = false;
        private bool _isClosing = false;
        private CancellationTokenSource? _readCancellationTokenSource;


        public event EventHandler<string> TerminalOutputUpdated;

        private string _host = string.Empty;
        public string Host
        {
            get => _host;
            set
            {
                if (_host != value)
                {
                    _host = value;
                    OnPropertyChanged();
                }
            }
        }

        private string _username = string.Empty;
        public string Username
        {
            get => _username;
            set
            {
                if (_username != value)
                {
                    _username = value;
                    OnPropertyChanged();
                }
            }
        }

        private string _password = string.Empty;
        public string Password
        {
            get => _password;
            set
            {
                if (_password != value)
                {
                    _password = value;
                    OnPropertyChanged();
                }
            }
        }

        private string _port = "22";
        public string Port
        {
            get => _port;
            set
            {
                if (_port != value)
                {
                    _port = value;
                    OnPropertyChanged();
                }
            }
        }

        private string _terminalOutput = string.Empty;
        public string TerminalOutput
        {
            get => _terminalOutput;
            set
            {
                if (_terminalOutput != value)
                {
                    _terminalOutput = value;
                    OnPropertyChanged();
                }
            }
        }

        private string _inputCommand = string.Empty;
        public string InputCommand
        {
            get => _inputCommand;
            set
            {
                if (_inputCommand != value)
                {
                    _inputCommand = value;
                    OnPropertyChanged();
                }
            }
        }

        private bool _inputVisible = false;
        public bool InputVisible
        {
            get => _inputVisible;
            set
            {
                if (_inputVisible != value)
                {
                    _inputVisible = value;
                    OnPropertyChanged();
                }
            }
        }

        private string _connectButtonText = "Połącz";
        public string ConnectButtonText
        {
            get => _connectButtonText;
            set
            {
                if (_connectButtonText != value)
                {
                    _connectButtonText = value;
                    OnPropertyChanged();
                }
            }
        }

        private bool _isConnecting = false;
        public bool IsConnecting
        {
            get => _isConnecting;
            set
            {
                if (_isConnecting != value)
                {
                    _isConnecting = value;
                    OnPropertyChanged();
                    ConnectCommand.NotifyCanExecuteChanged();
                }
            }
        }

        public IRelayCommand ConnectCommand { get; }
        public IRelayCommand SendCommandCommand { get; }

        public SSHViewModel()
        {
            Title = "SSH";
            ConnectCommand = new RelayCommand(ToggleConnection, () => !IsConnecting);
            SendCommandCommand = new RelayCommand(SendCommand);
        }

        public async void ToggleConnection()
        {
            if (_isConnected)
            {
                CloseConnection();
                ConnectButtonText = "Połącz";
                InputVisible = false;
                return;
            }

            if (string.IsNullOrEmpty(Host) || string.IsNullOrEmpty(Username))
            {
                MessageBox.Show("Proszę wprowadzić adres hosta i nazwę użytkownika.", "Błąd połączenia");
                return;
            }

            int portNumber = 22;
            if (!string.IsNullOrEmpty(Port) && int.TryParse(Port, out int customPort))
            {
                portNumber = customPort;
            }

            _sshClient = new SshClient(Host, portNumber, Username, Password);

            try
            {
                IsConnecting = true;
                ConnectButtonText = "Łączenie...";
                TerminalOutput = string.Empty;
                AppendOutput($"Łączenie z {Host}:{portNumber} jako {Username}...\r\n");

                await Task.Run(() => _sshClient.Connect());

                _isConnected = true;
                _shellStream = _sshClient.CreateShellStream("dumb", 80, 24, 800, 600, 1024);


                ConnectButtonText = "Rozłącz";
                InputVisible = true;
                AppendOutput("Połączono.\r\n");

                _readCancellationTokenSource = new CancellationTokenSource();
                var token = _readCancellationTokenSource.Token;

                _ = Task.Run(() => ReadOutputAsync(token), token);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Błąd połączenia: {ex.Message}", "Błąd");
                _isConnected = false;
                ConnectButtonText = "Połącz";
                AppendOutput($"Błąd: {ex.Message}\r\n");
            }
            finally
            {
                IsConnecting = false;
            }
        }

        private async Task ReadOutputAsync(CancellationToken cancellationToken)
        {
            byte[] buffer = new byte[1024];

            try
            {
                while (_isConnected && _shellStream != null && !_isClosing && !cancellationToken.IsCancellationRequested)
                {
                    if (_shellStream.DataAvailable)
                    {
                        int bytesRead = _shellStream.Read(buffer, 0, buffer.Length);
                        if (bytesRead == 0) continue;

                        string output = Encoding.UTF8.GetString(buffer, 0, bytesRead);
                        AppendOutput(output);
                    }
                    else
                    {
                        await Task.Delay(50, cancellationToken);
                    }
                }
            }
            catch (OperationCanceledException)
            {
            }
            catch (Exception ex)
            {
                if (!_isClosing && !cancellationToken.IsCancellationRequested)
                {
                    AppendOutput($"\r\nBłąd odczytu: {ex.Message}\r\n");
                    ConnectButtonText = "Połącz";
                    InputVisible = false;
                    _isConnected = false;
                }
            }
        }

        public void SendCommand()
        {
            if (_isConnected && _shellStream != null && !string.IsNullOrEmpty(InputCommand))
            {
                try
                {
                    string command = InputCommand + "\n";
                    AppendOutput($"> {InputCommand}\r\n");

                    byte[] commandBytes = Encoding.UTF8.GetBytes(command);
                    _shellStream.Write(commandBytes);
                    _shellStream.Flush();

                    InputCommand = string.Empty;
                }
                catch (Exception ex)
                {
                    AppendOutput($"\r\nBłąd wysyłania komendy: {ex.Message}\r\n");
                }
            }
        }

        private void AppendOutput(string text)
        {
            string processedText = text;
            TerminalOutputUpdated?.Invoke(this, processedText);
            Application.Current.Dispatcher.Invoke(() =>
            {
                TerminalOutput += processedText;
                OnPropertyChanged(nameof(TerminalOutput));
            });
        }

        private void CloseConnection()
        {
            _isClosing = true;
            _readCancellationTokenSource?.Cancel();

            if (_isConnected)
            {
                try
                {
                    if (_shellStream != null)
                    {
                        _shellStream.Close();
                        _shellStream.Dispose();
                        _shellStream = null;
                    }

                    if (_sshClient != null)
                    {
                        if (_sshClient.IsConnected)
                            _sshClient.Disconnect();

                        _sshClient.Dispose();
                        _sshClient = null;
                    }
                }
                catch (Exception)
                {
                }
                finally
                {
                    _isConnected = false;
                }
            }

            _isClosing = false;
            _readCancellationTokenSource?.Dispose();
            _readCancellationTokenSource = null;
        }

        public void Dispose()
        {
            CloseConnection();
        }
    }
}
