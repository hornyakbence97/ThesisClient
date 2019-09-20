using System;

namespace XamarinApp
{
    public static class Configuration
    {
        public static TimeSpan KeepAliveInterval = TimeSpan.FromMilliseconds(12000);
        public static Uri BaseUrlWebSocket = new Uri("wss://virtualnetwork.azurewebsites.net/ws?requestType=REQUEST");
        public static int ReceiveBufferSize = 4;
    }
}
