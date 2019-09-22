using System;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using XamarinApp.Exception;
using XamarinApp.Models.Dto.Input;
using XamarinApp.Models.Dto.Output;

namespace XamarinApp.Services
{
    public sealed class UserService
    {
        private static UserService instance = null;
        private static readonly object padlock = new object();

        private HttpClient _client;
        private UserDto _currentUser;

        UserService()
        {
            _client = new HttpClient
            {
                BaseAddress = Configuration.BaseUrl
            };
        }

        public static UserService Instance
        {
            get
            {
                lock (padlock)
                {
                    return instance ?? (instance = new UserService());
                }
            }
        }

        public async Task<UserDto> CreateUser(string friendlyName, int maxSpace)
        {
            var response = await _client.PostAsync(
                Configuration.CreateUserRelativeEndpoint + friendlyName + "/" + maxSpace,
                new StringContent(string.Empty, Encoding.UTF8, "application/json"),
                CancellationToken.None);

            if (!response.IsSuccessStatusCode)
            {
                throw new OperationFailedException("Failed to create user, API call unsuccessful");
            }

            var responseObj = JsonConvert.DeserializeObject<UserDto>((await response.Content.ReadAsStringAsync()));

            return responseObj;
        }

        public async Task<bool> AddUserToNetwork(string networkId, string networkPass)
        {
            var obj = new AddUserToNetworkInput
            {
                NetworkId = Guid.Parse(networkId),
                NetworkPassword = networkPass,
                UserId = GetCurrentUser().Token1
            };

            var response = await _client.PostAsync(
                Configuration.AddUserToNetworkRelativeEndpoint,
                new StringContent(JsonConvert.SerializeObject(obj), Encoding.UTF8, "application/json"),
                CancellationToken.None);

            if (!response.IsSuccessStatusCode)
            {
                throw new OperationFailedException("Failed to add user to network, API call unsuccessful");
            }

            _currentUser.NetworkId = obj.NetworkId;
            return true;
        }

        public void SetCurrentUser(UserDto user)
        {
            _currentUser = user;
        }

        public UserDto GetCurrentUser()
        {
            return _currentUser;
        }
    }
}
