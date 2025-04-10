using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Roomify.GP.Core.Entities.Identity;

namespace Roomify.GP.Core.Entities
{
    public class OtpCode
    {
        [Key]
        public Guid Id { get; set; }

        [Required]
        public Guid UserId { get; set; }

        [ForeignKey("UserId")]
        public ApplicationUser User { get; set; }

        [Required]
        public string Code { get; set; }

        [Required]
        public string Purpose { get; set; } // ConfirmEmail, ResetPassword, etc.

        [Required]
        public DateTime ExpirationTime { get; set; }

        public bool IsUsed { get; set; } = false;
    }
}
