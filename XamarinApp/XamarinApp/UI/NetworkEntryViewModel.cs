using System;
using System.IO;
using System.Threading.Tasks;
using Newtonsoft.Json;
using XamarinApp.Models.Dto.Input;
using XamarinApp.Services;

namespace XamarinApp.UI
{
    class NetworkEntryViewModel : BaseViewModel
    {
        private UserDto _user;

        public delegate void CanContinue(Guid networkId);
        public event CanContinue CanContinueEvent;

        private string _userName;
        private bool _isLoginFormVisible;
        private string _networkId;
        private string _networkName;
        private string _networkPassword;

        public string NetworkPassword
        {
            get => _networkPassword;
            set { _networkPassword = value; OnPropertyChanged();}
        }

        public string NetworkName
        {
            get => _networkName;
            set { _networkName = value; OnPropertyChanged();}
        }

        public string NetworkId
        {
            get => _networkId;
            set { _networkId = value; OnPropertyChanged();}
        }

        public bool IsLoginFormVisible
        {
            get => _isLoginFormVisible;
            set { _isLoginFormVisible = value; OnPropertyChanged();}
        }

        public string UserName
        {
            get => _userName;
            private set { _userName = value; OnPropertyChanged();}
        }

        public NetworkEntryViewModel()
        {
            _networkName = "Halozat" + DateTime.UtcNow.ToFileTimeUtc();
            _networkPassword = "123";
        }

        public void LoadCurrentUser()
        {
            IsBusy = true;
            _user = UserService.Instance.GetCurrentUser();
            UserName = _user.FriendlyName;
            IsBusy = false;
        }

        public async Task LoginToNetworkOrCreateNew()
        {
            IsBusy = true;
            if (!_user.NetworkId.HasValue)
            {
                IsLoginFormVisible = true;
                IsBusy = false;
                return;
            }

            await Login(autoFlow: true);

            IsBusy = false;
        }

        public async Task Login(bool newCreated = false, bool autoFlow = false)
        {
            var user = UserService.Instance.GetCurrentUser();
            string pwd;
            string id;

            pwd = autoFlow ? null : NetworkPassword;
            id = autoFlow? user.NetworkId.ToString() : NetworkId;

            if (newCreated)
            {
                pwd = NetworkPassword;
                id = user.NetworkId.Value.ToString();
            }

            await UserService.Instance.AddUserToNetwork(id, pwd);

            _user = UserService.Instance.GetCurrentUser();

            var fileName =
                Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                    Configuration.UserTextFileName);

            File.WriteAllText(fileName, JsonConvert.SerializeObject(_user));

            CanContinueEvent?.Invoke(_user.NetworkId.Value);
        }

        public async Task CreateNetwork()
        {
            IsBusy = true;

            var network = await NetworkService.Instance.CreateNetwork(NetworkName, NetworkPassword);

            var user = UserService.Instance.GetCurrentUser();
            user.NetworkId = network.NetworkId;
            UserService.Instance.SetCurrentUser(user);

            await Login(newCreated: true);
        }
    }
}
