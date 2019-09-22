using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace XamarinApp.Services
{
    public sealed class WebsocketService
    {
        private static WebsocketService instance = null;
        private static readonly object padlock = new object();

        private HttpClient _client;

        WebsocketService()
        {
            _client = new HttpClient
            {
                BaseAddress = Configuration.BaseUrl
            };
        }

        public static WebsocketService Instance
        {
            get
            {
                lock (padlock)
                {
                    return instance ?? (instance = new WebsocketService());
                }
            }
        }

        public async Task StartService(CancellationToken cancellationToken)
        {
            while (true)
            {
                await VirtualFileService.Instance.SendFilesToServer();

                await Task.Delay(3000);
            }
        }
    }
}
