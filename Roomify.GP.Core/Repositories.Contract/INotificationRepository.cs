using Roomify.GP.Core.Entities.Notification;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Roomify.GP.Core.Repositories.Contract
{
    public interface INotificationRepository
    {
        Task<Notification> CreateNotificationAsync(Notification notification);
        Task<List<Notification>> GetUserNotificationsAsync(Guid userId);
        Task<Notification> GetNotificationByIdAsync(Guid id);
        Task MarkAsReadAsync(Guid id);
        Task MarkAllAsReadAsync(Guid userId);
        Task DeleteNotificationAsync(Guid id);
    }
}
