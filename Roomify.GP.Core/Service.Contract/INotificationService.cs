using Roomify.GP.Core.DTOs.Notification;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Roomify.GP.Core.Service.Contract
{
    public interface INotificationService
    {
        Task<NotificationDto> CreateFollowNotificationAsync(Guid userId, Guid followerId, string followerName);
        Task<NotificationDto> CreateMessageNotificationAsync(Guid userId, Guid senderId, string senderName);
        Task<NotificationDto> CreateCommentNotificationAsync(Guid userId, Guid commenterId, string commenterName, Guid postId);
        Task<List<NotificationDto>> GetUserNotificationsAsync(Guid userId);
        Task MarkAsReadAsync(Guid id);
        Task MarkAllAsReadAsync(Guid userId);
        Task SendNotificationAsync(NotificationDto notification);
    }
}
