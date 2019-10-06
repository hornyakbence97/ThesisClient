using System;
using System.Collections.Generic;

namespace XamarinApp.WebSocket.Model.Dto.Input
{
    class SendFileDto : IncomingBaseDto
    {
        public List<Guid> FilePieceIds { get; set; }
    }
}
