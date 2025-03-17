using NexaFox.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace NexaFox.Views.Pages
{
    /// <summary>
    /// Logika interakcji dla klasy PortMonitor.xaml
    /// </summary>
    public partial class PortMonitor : UserControl
    {
        public PortMonitor()
        {
            InitializeComponent();
            this.DataContext = new PortMonitorViewModel();
        }
    }
}
