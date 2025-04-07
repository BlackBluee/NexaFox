using Renci.SshNet;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace NexaFox.Views.Pages
{
    public partial class ClientSSH : UserControl
    {
        public ClientSSH()
        {
            InitializeComponent();
            Loaded += ClientSSH_Loaded;
            Unloaded += ClientSSH_Unloaded;

            PasswordBox.PasswordChanged += PasswordBox_PasswordChanged;
            PasswordBox.GotFocus += PasswordBox_GotFocus;
            PasswordBox.LostFocus += PasswordBox_LostFocus;
        }

        private void ClientSSH_Loaded(object sender, RoutedEventArgs e)
        {
            UpdatePasswordPlaceholder();
        }

        private void ClientSSH_Unloaded(object sender, RoutedEventArgs e)
        {
            if (DataContext is IDisposable disposable)
                disposable.Dispose();
        }

        private void PasswordBox_GotFocus(object sender, RoutedEventArgs e)
        {
            PasswordPlaceholder.Visibility = Visibility.Collapsed;
        }

        private void PasswordBox_LostFocus(object sender, RoutedEventArgs e)
        {
            UpdatePasswordPlaceholder();
        }

        private void PasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
        {
            if (DataContext is NexaFox.ViewModels.SSHViewModel viewModel)
            {
                viewModel.Password = PasswordBox.Password;
            }

            UpdatePasswordPlaceholder();
        }

        private void UpdatePasswordPlaceholder()
        {
            PasswordPlaceholder.Visibility = string.IsNullOrEmpty(PasswordBox.Password)
                ? Visibility.Visible
                : Visibility.Collapsed;
        }
    }
}
