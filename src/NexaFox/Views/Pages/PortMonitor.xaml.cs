using NexaFox.ViewModels;
using System.Windows.Controls;

namespace NexaFox.Views.Pages
{
    public partial class PortMonitor : UserControl
    {
        private PortMonitorViewModel _viewModel;
        public PortMonitor()
        {
            InitializeComponent();
            _viewModel = new PortMonitorViewModel();
            DataContext = _viewModel;
        }
        private async void SearchButton_Click(object sender, RoutedEventArgs e)
        {
            await _viewModel.StartNetworkScan();
        }
    }
}
