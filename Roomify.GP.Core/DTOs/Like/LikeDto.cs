using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Roomify.GP.Core.DTOs.Like
{
    public class LikeDto
    {
        public Guid Id { get; set; }
        
        // User data
        public Guid UserId { get; set; }
        public string UserName { get; set; }
        public string UserProfilePicture { get; set; }

        public DateTime CreatedAt { get; set; }

        // Target reference
        public Guid? PortfolioPostId { get; set; }
        public Guid? SavedDesignId { get; set; }
    }
}
