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
    /// Logika interakcji dla klasy ClientSSH.xaml
    /// </summary>
    public partial class ClientSSH : UserControl
    {
        public ClientSSH()
        {
            InitializeComponent();
            this.DataContext = new SSHViewModel();
        }
    }
}
