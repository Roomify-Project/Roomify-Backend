using Roomify.GP.Core.Entities.AI;
using Roomify.GP.Core.Entities.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Roomify.GP.Core.Entities
{
    public class Like
    {
        public Guid Id { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        //Navigational Property
        public Guid ApplicationUserId { get; set; } // FK 
        public ApplicationUser ApplicationUser { get; set; }

        public Guid? PortfolioPostId { get; set; }  // FK
        public PortfolioPost PortfolioPost { get; set; }

        public Guid? SavedDesignId { get; set; }  // FK
        public SavedDesign SavedDesign { get; set; }
    }
}
