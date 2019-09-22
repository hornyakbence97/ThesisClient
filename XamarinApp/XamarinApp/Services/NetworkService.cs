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
    public sealed class NetworkService
    {
        private static NetworkService instance = null;
        private static readonly object padlock = new object();

        private HttpClient _client;
        private NetworkCreateDto _currentNetwork;

        NetworkService()
        {
            _client = new HttpClient
            {
                BaseAddress = Configuration.BaseUrl
            };
        }

        public static NetworkService Instance
        {
            get
            {
                lock (padlock)
                {
                    return instance ?? (instance = new NetworkService());
                }
            }
        }

        public async Task<NetworkCreateDto> CreateNetwork(string networkName, string networkPass)
        {
            var obj = new NetworkCreateInput
            {
                NetworkName = networkName,
                NetworkPassword = networkPass
            };

            var response = await _client.PostAsync(
                Configuration.CreateNetworkRelativeEndpoint,
                new StringContent(JsonConvert.SerializeObject(obj), Encoding.UTF8, "application/json"),
                CancellationToken.None);

            if (!response.IsSuccessStatusCode)
            {
                throw new OperationFailedException("Failed to create network, API call unsuccessful");
            }

            var responseObj = JsonConvert.DeserializeObject<NetworkCreateDto>((await response.Content.ReadAsStringAsync()));

            return responseObj;
        }

        public void SetCurrentNetwork(NetworkCreateDto network)
        {
            _currentNetwork = network;
        }

        public NetworkCreateDto GetCurrentUser()
        {
            return _currentNetwork;
        }
    }
}
