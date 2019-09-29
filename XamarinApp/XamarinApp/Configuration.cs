using System;
using Java.Lang;

namespace XamarinApp
{
    public static class Configuration
    {
        public static TimeSpan KeepAliveInterval = TimeSpan.FromMilliseconds(12000);
        public static Uri BaseUrlWebSocket = new Uri("wss://virtualnetwork.azurewebsites.net/ws?requestType=REQUEST");
        public static int ReceiveBufferSize = 4;
        public static Uri BaseUrl = new Uri("https://virtualnetwork.azurewebsites.net");
        public static string CreateUserRelativeEndpoint = "User/CreateUser/";
        public static string CreateNetworkRelativeEndpoint = "Network/Create";
        public static string UserTextFileName = "user.txt";
        public static string AddUserToNetworkRelativeEndpoint = "Network/AddUser";
        public static string DefaultFileFolder = "NetworkData";
        public static string SendFileListToServerRelativeEndpoint = "Files/UploadIdList/";
        public static string NotificationChannelName = "VNetwork";
        public static string NotificationChannelDescription = "Virtual network description in config";
        public static string NotificationContentTitle = "Virtual network";
        public static string NotificationContentText = "The network is running in the background";
        public static string GetFileListRelativeEndpoint = "Files/Fetch";
    }
}
