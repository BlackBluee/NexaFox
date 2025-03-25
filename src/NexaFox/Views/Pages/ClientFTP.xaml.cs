using NexaFox.ViewModels;
using System.Windows.Controls;
namespace NexaFox.Views.Pages
{
    public partial class ClientFTP : UserControl
    {
        public ClientFTP()
        {
            InitializeComponent();
            this.DataContext = new FTPViewModel();
        }

        
    }
}
