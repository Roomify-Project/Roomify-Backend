using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Roomify.GP.Core.Entities.Identity
{
    public class UserConnection
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public Guid UserId { get; set; }
        public string ConnectionId { get; set; }
        public DateTime ConnectedAt { get; set; } = DateTime.UtcNow;
    }
}
