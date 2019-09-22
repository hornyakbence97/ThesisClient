using System.Net.Http;

namespace XamarinApp.Services
{
    public sealed class EventsClass
    {
        private static EventsClass instance = null;
        private static readonly object padlock = new object();

        private HttpClient _client;

        EventsClass()
        {
            _client = new HttpClient
            {
                BaseAddress = Configuration.BaseUrl
            };
        }

        public static EventsClass Instance
        {
            get
            {
                lock (padlock)
                {
                    return instance ?? (instance = new EventsClass());
                }
            }
        }
    }
}