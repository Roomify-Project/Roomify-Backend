using Microsoft.AspNetCore.Identity;

namespace Roomify.GP.Core.Entities.Identity
{
    public class ApplicationUser : IdentityUser<Guid>
    {
        public string FullName { get; set; }
        public string Bio { get; set; }
        public string? ProfilePicture { get; set; }
        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
        public bool EmailConfirmed { get; set; } = false;
        public ICollection<UserFollow> Followers { get; set; }
        public ICollection<UserFollow> Following { get; set; }


        public string? Provider { get; set; }
        public string? ProviderId { get; set; }
    }
}
