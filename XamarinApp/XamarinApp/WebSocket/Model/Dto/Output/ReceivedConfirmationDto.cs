using System;
using XamarinApp.WebSocket.Model.Dto.Input;

namespace XamarinApp.WebSocket.Model.Dto.Output
{
    public class ReceivedConfirmationDto : BaseDto
    {
        public Guid ReceiveId { get; set; }
        public IncomingRequestType Type { get; set; }
    }
}
