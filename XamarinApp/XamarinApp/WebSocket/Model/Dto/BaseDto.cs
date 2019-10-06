using System;

namespace XamarinApp.WebSocket.Model.Dto
{
    public class BaseDto
    {
        public Guid Token1 { get; set; }
        public WebSocketRequestType RequestType { get; set; }
    }
}
