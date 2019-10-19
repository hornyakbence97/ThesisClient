using System;

namespace XamarinApp.WebSocket.Model.Dto.Input
{
    public class DeleteFilePeaceDto : IncomingBaseDto
    {
        public Guid FilePeaceId { get; set; }
    }
}
