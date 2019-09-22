using System;
using System.IO;
using System.Threading.Tasks;
using Newtonsoft.Json;
using XamarinApp.Exception;
using XamarinApp.Models.Dto.Input;
using XamarinApp.Services;

namespace XamarinApp.UI
{
    public class MainPageViewModel : BaseViewModel
    {
        private bool _isBusy;
        private string _showText;
        private string _fileName;

        public delegate void CanContinue();
        public event CanContinue CanContinueEvent;

        public delegate void RequiresMoreInformation();
        public event RequiresMoreInformation MoreInformationNeeded;


        public bool IsBusy
        {
            get => _isBusy;
            set { _isBusy = value; OnPropertyChanged();}
        }

        public string ShowText
        {
            get => _showText;
            set { _showText = value; OnPropertyChanged();}
        }

        public MainPageViewModel()
        {
            _fileName =
                Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                    Configuration.UserTextFileName);
        }

        public void LoadUser()
        {
            ShowText = "Getting user details...";
            IsBusy = true;
            if (!File.Exists(_fileName))
            {
                MoreInformationNeeded?.Invoke();

                return;
            }

            var text = File.ReadAllText(_fileName);

            var currentUser = JsonConvert.DeserializeObject<UserDto>(text);

            ShowText = "Signing in...";

            UserService.Instance.SetCurrentUser(currentUser);

            //await Task.Delay(10);
            CanContinueEvent?.Invoke();

            //return Task.CompletedTask;
        }

        public async Task AddMoreInformation(string friendlyName, int maxSpace)
        {
            ShowText = "Creating new user...";

            var user = await UserService.Instance.CreateUser(friendlyName, maxSpace);

            if (user != null)
            {
                File.WriteAllText(_fileName, JsonConvert.SerializeObject(user));

                ShowText = "Signing in...";

                UserService.Instance.SetCurrentUser(user);

                IsBusy = false;

                CanContinueEvent?.Invoke();

                return;
            }

            throw new OperationFailedException("User creation failed.");
        }
    }
}
