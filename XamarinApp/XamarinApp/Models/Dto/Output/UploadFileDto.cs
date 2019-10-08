using System;

namespace XamarinApp.Models.Dto.Output
{
    public class UploadFileDto
    {
        public Guid UserToken1 { get; set; }
        public string FileName { get; set; }
        public string MimeType { get; set; }
    }
}
