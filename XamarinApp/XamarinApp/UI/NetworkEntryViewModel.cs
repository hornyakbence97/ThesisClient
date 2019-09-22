using XamarinApp.Services;

namespace XamarinApp.UI
{
    class NetworkEntryViewModel : BaseViewModel
    {
        private string _userName;

        public string UserName
        {
            get => _userName;
            private set { _userName = value; OnPropertyChanged();}
        }

        public void LoadCurrentUser()
        {
            UserName = UserService.Instance.GetCurrentUser().FriendlyName;
        }
    }
}
