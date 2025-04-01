namespace Roomify.GP.Core.Entities.Identity
{
    public class EmailConfirmationToken
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string Code { get; set; } = null!;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime ExpiryAt { get; set; }
        public bool IsUsed { get; set; } = false;

        // Foreign Key to ApplicationUser
        public string UserId { get; set; } = null!;
        public ApplicationUser User { get; set; } = null!;
    }
}
