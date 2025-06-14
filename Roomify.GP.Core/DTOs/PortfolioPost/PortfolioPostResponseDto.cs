using Roomify.GP.Core.DTOs.Comment;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Roomify.GP.Core.DTOs.PortfolioPost
{
    public class PortfolioPostResponseDto
    {
        public Guid Id { get; set; }
        public string ImagePath { get; set; }
        public string Description { get; set; }
        public DateTime CreatedAt { get; set; }

        // User data
        public Guid UserId { get; set; }
        public string UserName { get; set; }
        public string UserProfilePicture { get; set; }

        // Social features
        public List<CommentResponseDto> Comments { get; set; }
        public int LikesCount { get; set; }
        public bool IsLiked { get; set; }


    }
}   