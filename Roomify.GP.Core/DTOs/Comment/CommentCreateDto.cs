using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Roomify.GP.Core.DTOs.Comment
{
    public class CommentCreateDto
    {
        public string Content { get; set; }
        public Guid PortfolioPostId { get; set; }
        public Guid ApplicationUserId { get; set; }

    }
}
