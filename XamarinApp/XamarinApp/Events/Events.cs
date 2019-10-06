namespace XamarinApp.Events
{
    public static class Events
    {
        public static string StartLongRunningTask = "StartLongRunningTask";
        public static string StopLongRunningTask = "StopLongRunningTask";
        public static string CancelledTask = "CancelledTask";
        public static string Message = "Message";
        public static string WebSocketReceive = "WebSocketReceive";

        public static string StartService = "StartService";
        public static string StopService = "StopService";
        public static string FileReceived = "FileReceived";
    }
}
