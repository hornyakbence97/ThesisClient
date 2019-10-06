using System;
using System.Collections.Generic;

namespace XamarinApp.WebSocket.Model.Dto.Input
{
    class SaveFileDto: IncomingBaseDto
    {
        public List<(byte[] Bytes, Guid Id)> FilePeaces { get; set; }
    }
}
