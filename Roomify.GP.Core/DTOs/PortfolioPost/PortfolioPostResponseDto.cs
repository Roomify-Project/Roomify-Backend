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
        public Guid ApplicationUserId { get; set; }
    }
}
