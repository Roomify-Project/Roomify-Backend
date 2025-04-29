using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Roomify.GP.Core.DTOs.Comment
{
    public class CommentResponseDto
    {
        public Guid Id { get; set; }
        public string Content { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }

        // User info
        public Guid UserId { get; set; }
        public string UserName { get; set; }
        public string UserProfilePicture { get; set; }

        // Post info
        public Guid PortfolioPostId { get; set; }
    }
}
