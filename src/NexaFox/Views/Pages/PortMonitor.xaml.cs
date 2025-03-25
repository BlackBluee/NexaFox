using NexaFox.ViewModels;
using System.Windows.Controls;

namespace NexaFox.Views.Pages
{
    public partial class PortMonitor : UserControl
    {
        public PortMonitor()
        {
            InitializeComponent();
            this.DataContext = new PortMonitorViewModel();
        }
    }
}
