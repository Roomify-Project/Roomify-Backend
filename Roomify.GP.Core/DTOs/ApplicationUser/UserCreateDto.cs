using Roomify.GP.Core.Enums;

namespace Roomify.GP.Core.DTOs.ApplicationUser

{
    public class UserCreateDto
    {
        public string FullName { get; set; }
        public string UserName { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public string? Bio { get; set; }
        public string? ProfilePicture { get; set; } = "default.jpg";
        public string Roles { get; set; }

    }

}
