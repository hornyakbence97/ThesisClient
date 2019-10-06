using System;

namespace XamarinApp.Models.Dto.Output
{
    public class DeleteFileDto
    {
        public Guid FileId { get; set; }
        public Guid UserToken1Id { get; set; }
    }
}
