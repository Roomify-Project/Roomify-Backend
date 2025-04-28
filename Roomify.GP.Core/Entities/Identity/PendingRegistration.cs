using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Roomify.GP.Core.Entities.Identity
{
    [Table("PendingRegistrations")]
    public class PendingRegistration
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        public string FullName { get; set; }

        [Required]
        public string UserName { get; set; }

        [Required]
        [EmailAddress]
        public string Email { get; set; }

        public string Bio { get; set; }

        [Required]
        public string Password { get; set; }

        [Required]
        public string Roles { get; set; }

        public DateTime CreatedDate { get; set; }

        public DateTime ExpiryDate { get; set; }
    }
}
