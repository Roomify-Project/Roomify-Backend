using Microsoft.AspNetCore.SignalR;
using Roomify.GP.API.Hubs;
using Roomify.GP.Core.Service.Contract;

namespace Roomify.GP.API.Services
{
    public class NotificationBroadcaster : INotificationBroadcaster
    {
        private readonly IHubContext<NotificationHub> _hubContext;

        public NotificationBroadcaster(IHubContext<NotificationHub> hubContext)
        {
            _hubContext = hubContext;
        }

        public async Task BroadcastNotificationAsync(Guid recipientUserId, string type, string message, Guid? relatedEntityId = null)
        {
            await _hubContext.Clients.Group(recipientUserId.ToString())
                .SendAsync("ReceiveNotification", new
                {
                    type,
                    message,
                    isRead = false,
                    createdAt = DateTime.UtcNow,
                    relatedEntityId
                });
        }
    }
}
