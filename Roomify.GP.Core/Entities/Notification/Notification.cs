using Roomify.GP.Core.Entities.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Roomify.GP.Core.Entities.Notification;
public class Notification
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public ApplicationUser User { get; set; }
    public string Message { get; set; }
    public bool IsRead { get; set; } = false;
    public NotificationType Type { get; set; }
    public Guid? RelatedItemId { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

public enum NotificationType
{
    Follow,
    Message,
    Comment
}