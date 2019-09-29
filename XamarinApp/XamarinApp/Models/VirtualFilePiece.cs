using System;

namespace XamarinApp.Models
{
    public class VirtualFilePiece
    {
        public string Id { get; set; }
        public long FileSize { get; set; }
        public DateTime LastModified { get; set; }  
    }
}
