using Roomify.GP.Core.Entities.AI.RoomImage;
using Roomify.GP.Core.Entities.Identity;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
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
        public Guid ApplicationUserId { get; set; }  // Changed to Guid
        public virtual ApplicationUser ApplicationUser { get; set; }


        public Guid PromptId { get; set; }
        public virtual Prompt Prompt { get; set; }
        


        //Image will be removed from history automatically after 72 hours
        [NotMapped]  // to prevent EF from trying to map it
        public bool IsExpired => DateTime.UtcNow > CreatedAt.AddHours(72);
    }
}
