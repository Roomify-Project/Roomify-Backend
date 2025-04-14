using Roomify.GP.Core.Entities.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Roomify.GP.Core.Entities
{
    public class UserFollow
    {
        public Guid FollowerId { get; set; }  // اللي بيعمل Follow
        public Guid FollowingId { get; set; } // اللي بيتعمله Follow

        public ApplicationUser Follower { get; set; } = null!;
        public ApplicationUser Following { get; set; } = null!;

        public DateTime FollowedAt { get; set; } = DateTime.UtcNow;

    }
}