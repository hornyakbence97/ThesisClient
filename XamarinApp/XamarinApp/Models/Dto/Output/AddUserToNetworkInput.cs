using System;

namespace XamarinApp.Models.Dto.Output
{
    class AddUserToNetworkInput
    {
        public Guid UserId { get; set; }
        public Guid NetworkId { get; set; }
        public string NetworkPassword { get; set; }
    }
}
