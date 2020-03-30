using System;

namespace XamarinApp.Models
{
    public class 
        VirtualFile
    {
        public Guid FileId { get; set; }
        public string FileName { get; set; }
        public long FileSize { get; set; }
        public DateTime LastModified { get; set; }
        public string UploadedBy { get; set; }
        public string MimeType { get; set; }
        public DateTime Created { get; set; }
        public string ModifiedBy { get; set; }
        public bool IsConfirmed { get; set; }

        public string GetFileSize => Math.Round((double)FileSize / 1024 / 1024, MidpointRounding.ToEven)  + " MB";
        public string GetUploadTime => Created.ToString("g");
        public string GetUploadedBy => "Uploaded by: " + UploadedBy;
    }
}
