using System;

namespace Roomify.GP.Core.DTOs.User
{
    public class UserResponseDto
    {
        public Guid ID { get; set; }
        public string FullName { get; set; }
        public string Email { get; set; }
        public string UserName { get; set; }
        public string Bio { get; set; }
        public string ProfilePicture { get; set; }
        public string Roles { get; set; }
        public DateTime CreatedDate { get; set; }
    }
}
