using XamarinApp.Models.Dto.Output;

namespace XamarinApp.Models
{
    public class UploadServiceModel : UploadFileDto
    {
        public byte[] Bytes { get; set; }
    }
}
