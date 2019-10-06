using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using XamarinApp.WebSocket;

namespace XamarinApp.Services
{
    public sealed class WebsocketService
    {
        private static WebsocketService instance = null;
        private static readonly object padlock = new object();

        private HttpClient _client;
        private WebSocketProcesser _webSocketProcesser = new WebSocketProcesser();

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
            await _webSocketProcesser.StartTask();
        }
    }
}
