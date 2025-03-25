using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace NexaFox.ViewModels
{
    public abstract class TabContentViewModelBase : INotifyPropertyChanged
    {
        private string _title;
        public string Title
        {
            get => _title;
            set
            {
                _title = value;
                OnPropertyChanged();
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
