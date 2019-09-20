using System;
using System.Collections.Generic;
using System.Text;

namespace XamarinApp.Events
{
    public static class Events
    {
        public static string StartLongRunningTask = "StartLongRunningTask";
        public static string StopLongRunningTask = "StopLongRunningTask";
        public static string CancelledTask = "CancelledTask";
        public static string Message = "Message";
        public static string WebSocketReceive = "WebSocketReceive";
    }
}
