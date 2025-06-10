using Roomify.GP.Core.Entities.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Roomify.GP.Core.Entities.AI
{
    public class SavedDesign 
    {
        public Guid Id { get; set; }
        public string GeneratedImageUrl { get; set; }
        public DateTime SavedAt { get; set; }

        // Navigational Property
        public Guid ApplicationUserId { get; set; }
        public virtual ApplicationUser ApplicationUser { get; set; }

        // Comments on saved designs
        public ICollection<Comment> Comments { get; set; } = new List<Comment>();

        // Likes on saved designs
        public ICollection<Like> Likes { get; set; } = new List<Like>();
    }
}
