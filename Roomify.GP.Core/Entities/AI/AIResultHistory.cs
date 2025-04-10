using Roomify.GP.Core.Entities.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Roomify.GP.Core.Entities.AI
{
    public class AIResultHistory
    {
        public Guid Id { get; set; }
        public string GeneratedImageUrl { get; set; }
        public DateTime CreatedAt { get; set; }

        // Navigational Property
        public string ApplicationUserId { get; set; }
        public virtual ApplicationUser ApplicationUser { get; set; }


        //Image will be removed from history automatically after 72 hours
        public bool IsExpired => DateTime.UtcNow > CreatedAt.AddHours(72);
    }
}
