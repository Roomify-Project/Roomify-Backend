using Microsoft.AspNetCore.Identity;
using Roomify.GP.Core.Enums;

namespace Roomify.GP.Core.Entities.Identity
{
    public class ApplicationUser : IdentityUser
    {
        public string FullName { get; set; }
        public string Bio { get; set; }
        public string? ProfilePicture { get; set; }
        public Roles Roles { get; set; }
        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
        public bool EmailConfirmed { get; set; } = false;


        public string? Provider { get; set; }
        public string? ProviderId { get; set; }
    }
}
