using Roomify.GP.Core.Entities;
using Roomify.GP.Core.Repositories.Contract;
using Roomify.GP.Core.Service.Contract;

namespace Roomify.GP.Service.Services
{
    public class NotificationService : INotificationService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly INotificationBroadcaster _broadcaster;

        public NotificationService(IUnitOfWork unitOfWork, INotificationBroadcaster broadcaster)
        {
            _unitOfWork = unitOfWork;
            _broadcaster = broadcaster;
        }

        public async Task<IEnumerable<Notification>> GetUserNotificationsAsync(Guid userId)
        {
            return await _unitOfWork.Repository<Notification>()
                .GetAllAsync(
                    filter: n => n.RecipientUserId == userId,
                    orderBy: q => q.OrderByDescending(n => n.CreatedAt)
                );
        }

        public async Task<Notification> CreateNotificationAsync(Guid recipientUserId, string type, string message, Guid? relatedEntityId = null)
        {
            var notification = new Notification
            {
                RecipientUserId = recipientUserId,
                Type = type,
                Message = message,
                RelatedEntityId = relatedEntityId,
                IsRead = false,
                CreatedAt = DateTime.UtcNow
            };

            await _unitOfWork.Repository<Notification>().AddAsync(notification);
            await _unitOfWork.SaveAsync();

            // إرسال عبر SignalR باستخدام Broadcaster
            await _broadcaster.BroadcastNotificationAsync(recipientUserId, type, message, relatedEntityId);

            return notification;
        }

        public async Task MarkAsReadAsync(Guid notificationId)
        {
            var repo = _unitOfWork.Repository<Notification>();
            var notification = await repo.GetByIdAsync(notificationId);
            if (notification != null)
            {
                notification.IsRead = true;
                await _unitOfWork.SaveAsync();
            }
        }

        public async Task MarkAllAsReadAsync(Guid userId)
        {
            var repo = _unitOfWork.Repository<Notification>();
            var notifications = await repo.GetAllAsync(n => n.RecipientUserId == userId && !n.IsRead);

            foreach (var n in notifications)
                n.IsRead = true;

            await _unitOfWork.SaveAsync();
        }
    }
}
