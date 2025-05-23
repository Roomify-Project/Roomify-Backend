using AutoMapper;
using Roomify.GP.Core.DTOs.Notification;
using Roomify.GP.Core.Entities.Notification;
using Roomify.GP.Core.Repositories.Contract;
using Roomify.GP.Core.Service.Contract;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Roomify.GP.Service.Services
{
    public class NotificationService : INotificationService
    {
        private readonly INotificationRepository _notificationRepository;
        private readonly IMapper _mapper;
        private readonly INotificationSender _notificationSender;

        public NotificationService(
            INotificationRepository notificationRepository,
            IMapper mapper,
            INotificationSender notificationSender)
        {
            _notificationRepository = notificationRepository;
            _mapper = mapper;
            _notificationSender = notificationSender;
        }

        public async Task<NotificationDto> CreateFollowNotificationAsync(Guid userId, Guid followerId, string followerName)
        {
            var notification = new Notification
            {
                UserId = userId,
                Message = $"{followerName} started following you",
                Type = NotificationType.Follow,
                RelatedItemId = followerId
            };

            var createdNotification = await _notificationRepository.CreateNotificationAsync(notification);
            var notificationDto = _mapper.Map<NotificationDto>(createdNotification);

            // إرسال الإشعار
            await _notificationSender.NotifyUserAsync(
                userId,
                notificationDto.Message,
                notificationDto);

            return notificationDto;
        }

        public async Task<NotificationDto> CreateMessageNotificationAsync(Guid userId, Guid senderId, string senderName)
        {
            var notification = new Notification
            {
                UserId = userId,
                Message = $"You received a new message from {senderName}",
                Type = NotificationType.Message,
                RelatedItemId = senderId
            };

            var createdNotification = await _notificationRepository.CreateNotificationAsync(notification);
            var notificationDto = _mapper.Map<NotificationDto>(createdNotification);

            await _notificationSender.NotifyUserAsync(
                userId,
                notificationDto.Message,
                notificationDto);

            return notificationDto;
        }

        public async Task<NotificationDto> CreateCommentNotificationAsync(Guid userId, Guid commenterId, string commenterName, Guid postId)
        {
            var notification = new Notification
            {
                UserId = userId,
                Message = $"{commenterName} commented on your post",
                Type = NotificationType.Comment,
                RelatedItemId = postId
            };

            var createdNotification = await _notificationRepository.CreateNotificationAsync(notification);
            var notificationDto = _mapper.Map<NotificationDto>(createdNotification);

            await _notificationSender.NotifyUserAsync(
                userId,
                notificationDto.Message,
                notificationDto);

            return notificationDto;
        }

        public async Task<List<NotificationDto>> GetUserNotificationsAsync(Guid userId)
        {
            var notifications = await _notificationRepository.GetUserNotificationsAsync(userId);
            return _mapper.Map<List<NotificationDto>>(notifications);
        }

        public async Task MarkAsReadAsync(Guid id)
        {
            await _notificationRepository.MarkAsReadAsync(id);
        }

        public async Task MarkAllAsReadAsync(Guid userId)
        {
            await _notificationRepository.MarkAllAsReadAsync(userId);
        }

        public async Task SendNotificationAsync(NotificationDto notification)
        {
            await _notificationSender.NotifyUserAsync(
                notification.UserId,
                notification.Message,
                notification);
        }
    }
}
