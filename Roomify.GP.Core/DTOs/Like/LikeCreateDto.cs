using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Roomify.GP.Core.DTOs.Like
{
    public class LikeCreateDto
    {
        public Guid UserId { get; set; }  
        public Guid? PortfolioPostId { get; set; }
        public Guid? SavedDesignId { get; set; }
    }
}
