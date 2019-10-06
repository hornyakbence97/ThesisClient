using System;

namespace XamarinApp.WebSocket.Model.Dto.Output
{
    public class AuthenticationDto : BaseDto
    {
        public Guid Token2 { get; set; }
        public string FriendlyName { get; set; }
    }
}
