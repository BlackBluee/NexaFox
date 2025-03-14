using CommunityToolkit.Mvvm.ComponentModel;

namespace NexaFox.ViewModels
{
    public partial class TabItemViewModel : ObservableObject
    {
        [ObservableProperty]
        private string _header = "Nowa karta";

        [ObservableProperty]
        private object _content;
    }
}