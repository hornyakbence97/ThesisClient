using System;
using System.Collections.Generic;

namespace XamarinApp.WebSocket.Model.Dto.Input
{
    class DeleteFileDto : IncomingBaseDto
    {
        public List<Guid> FilePiecesToDelete { get; set; }
    }
}
