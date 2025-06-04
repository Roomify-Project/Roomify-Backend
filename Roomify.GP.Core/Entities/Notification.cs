using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Roomify.GP.Core.Entities
{
    public class Notification
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        public Guid RecipientUserId { get; set; } // اللي بيجيله النوتيفيكيشن
        public string Type { get; set; } // Follow, Comment, Reply, Message
        public string Message { get; set; }
        public bool IsRead { get; set; } = false;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public Guid? RelatedEntityId { get; set; } // optional: postId / messageId / commentId
    }

}
