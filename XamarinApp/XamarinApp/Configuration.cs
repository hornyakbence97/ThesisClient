using System;

namespace XamarinApp
{
    public static class Configuration
    {
        public static TimeSpan KeepAliveInterval = TimeSpan.FromMilliseconds(12000);
        public static Uri BaseUrlWebSocket = new Uri("wss://virtualnetwork.azurewebsites.net/ws?requestType=REQUEST");
        public static int ReceiveBufferSize = 4;
        public static Uri BaseUrl = new Uri("https://virtualnetwork.azurewebsites.net");
        public static string CreateUserRelativeEndpoint = "User/CreateUser/";
        public static string UserTextFileName = "user.txt";
    }
}
