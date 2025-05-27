using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Roomify.GP.Core.Entities.AI
{
    public class AIResult
    {
        public Guid Id { get; set; }
        public string Status { get; set; }
        public string GeneratedImageUrl { get; set; }
    }
}
