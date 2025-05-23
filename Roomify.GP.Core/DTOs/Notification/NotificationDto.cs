using Roomify.GP.Core.Entities.Notification;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Roomify.GP.Core.DTOs.Notification;
public class NotificationDto
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string Message { get; set; }
    public bool IsRead { get; set; }
    public NotificationType Type { get; set; }
    public Guid? RelatedItemId { get; set; }
    public DateTime CreatedAt { get; set; }
}