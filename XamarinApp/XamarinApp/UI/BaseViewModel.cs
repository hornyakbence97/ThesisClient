using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace XamarinApp.UI
{
    public class BaseViewModel : INotifyPropertyChanged
    {
        private bool _isBusy;
        private string _showText;

        public bool IsBusy
        {
            get => _isBusy;
            set { _isBusy = value; OnPropertyChanged(); }
        }

        public string ShowText
        {
            get => _showText;
            set { _showText = value; OnPropertyChanged(); }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string sender = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(sender));
        }
    }
}
