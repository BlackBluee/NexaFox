using NexaFox.ViewModels;
using System.Windows.Controls;
using System.Windows;
using NexaFox.Services;


namespace NexaFox.Views.Pages
{
    public partial class PortMonitor : UserControl
    {
        private PortMonitorViewModel _viewModel;
        public PortMonitor()
        {
            InitializeComponent();
            var scannerService = new NetworkScannerService();
            _viewModel = new PortMonitorViewModel(scannerService);
            DataContext = _viewModel;
        }
    }
}
