using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Roomify.GP.Core.DTOs.PortfolioPost
{
    public class PortfolioPostDto
    {
        public IFormFile ImageFile { get; set; }    // the uploaded file 
        public string Description { get; set; }
    }
}
