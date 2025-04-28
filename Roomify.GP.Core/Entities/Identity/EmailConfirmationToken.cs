namespace Roomify.GP.Core.Entities.Identity
{
    public class EmailConfirmationToken
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string Code { get; set; }  
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime ExpiryAt { get; set; }
        public bool IsUsed { get; set; } = false;
        public Guid? PendingRegistrationId { get; set; }
        public PendingRegistration? PendingRegistration { get; set; } = null!;

        // Foreign Key to ApplicationUser
        public Guid? UserId { get; set; } 
        public ApplicationUser? User { get; set; } = null!;
    }
}
    