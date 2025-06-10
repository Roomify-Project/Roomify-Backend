using Roomify.GP.Core.Entities.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Roomify.GP.Core.Entities
{
    public class PortfolioPost
    {
        public Guid Id { get; set; }
        public string ImagePath { get; set; }   // the saved file path/URL
        public string Description { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public bool IsDeleted { get; set; }

        // FK
        public Guid ApplicationUserId { get; set; }
        public required ApplicationUser ApplicationUser { get; set; }

        // Comments on Portfolio Post
        public ICollection<Comment> Comments { get; set; } = new List<Comment>();

        // Likes on Portfolio Post
        public ICollection<Like> Likes { get; set; } = new List<Like>();

    }
}