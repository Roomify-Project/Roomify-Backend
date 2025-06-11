using Roomify.GP.Core.Entities.AI;
using Roomify.GP.Core.Entities.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Roomify.GP.Core.Entities
{
    public class Comment
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        // Content of the comment
        public string Content { get; set; }

        // Timestamps
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }

        // Soft delete
        public bool IsDeleted { get; set; } = false;

        // Foreign Keys
        public Guid ApplicationUserId { get; set; }
        public Guid? PortfolioPostId { get; set; }
        public Guid? SavedDesignId { get; set; }

        // Navigation Properties
        public ApplicationUser ApplicationUser { get; set; }
        public PortfolioPost PortfolioPost { get; set; }
        public SavedDesign SavedDesign { get; set; }
    }
}
