using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Roomify.GP.Core.Service.Contract;
using Roomify.GP.API.Errors;
using System.Security.Claims;

namespace Roomify.GP.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class NotificationsController : ControllerBase
    {
        private readonly INotificationService _notificationService;

        public NotificationsController(INotificationService notificationService)
        {
            _notificationService = notificationService;
        }

        [HttpGet]
        public async Task<IActionResult> GetUserNotifications()
        {
            try
            {
                var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
                var notifications = await _notificationService.GetUserNotificationsAsync(userId);

                return Ok(new
                {
                    status = true,
                    message = "Notifications fetched successfully.",
                    data = notifications
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiErrorResponse(500, $"Failed to fetch notifications: {ex.Message}"));
            }
        }

        [HttpPut("{id}/read")]
        public async Task<IActionResult> MarkAsRead(Guid id)
        {
            try
            {
                await _notificationService.MarkAsReadAsync(id);
                return Ok(new
                {
                    status = true,
                    message = "Notification marked as read."
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiErrorResponse(500, $"Failed to mark as read: {ex.Message}"));
            }
        }

        [HttpPut("read-all")]
        public async Task<IActionResult> MarkAllAsRead()
        {
            try
            {
                var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
                await _notificationService.MarkAllAsReadAsync(userId);

                return Ok(new
                {
                    status = true,
                    message = "All notifications marked as read."
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiErrorResponse(500, $"Failed to mark all as read: {ex.Message}"));
            }
        }

        [HttpPost("test/follow")]
        public async Task<IActionResult> SendFollowNotification([FromQuery] Guid targetUserId)
        {
            try
            {
                var senderName = User.FindFirstValue(ClaimTypes.Name) ?? "Someone";

                var result = await _notificationService.CreateNotificationAsync(
                    recipientUserId: targetUserId,
                    type: "Follow",
                    message: $"{senderName} started following you."
                );

                return Ok(new { status = true, message = "Follow test sent", data = result });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiErrorResponse(500, $"Failed to send follow notification: {ex.Message}"));
            }
        }

        [HttpPost("test/comment")]
        public async Task<IActionResult> SendCommentNotification([FromQuery] Guid targetUserId, [FromQuery] Guid postId)
        {
            try
            {
                var senderName = User.FindFirstValue(ClaimTypes.Name) ?? "Someone";

                var result = await _notificationService.CreateNotificationAsync(
                    recipientUserId: targetUserId,
                    type: "Comment",
                    message: $"{senderName} commented on your post.",
                    relatedEntityId: postId
                );

                return Ok(new { status = true, message = "Comment test sent", data = result });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiErrorResponse(500, $"Failed to send comment notification: {ex.Message}"));
            }
        }

        [HttpPost("test/reply")]
        public async Task<IActionResult> SendReplyNotification([FromQuery] Guid targetUserId, [FromQuery] Guid commentId)
        {
            try
            {
                var senderName = User.FindFirstValue(ClaimTypes.Name) ?? "Someone";

                var result = await _notificationService.CreateNotificationAsync(
                    recipientUserId: targetUserId,
                    type: "Reply",
                    message: $"{senderName} replied to your comment.",
                    relatedEntityId: commentId
                );

                return Ok(new { status = true, message = "Reply test sent", data = result });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiErrorResponse(500, $"Failed to send reply notification: {ex.Message}"));
            }
        }

        [HttpPost("test/message")]
        public async Task<IActionResult> SendMessageNotification([FromQuery] Guid targetUserId, [FromQuery] Guid messageId)
        {
            try
            {
                var senderName = User.FindFirstValue(ClaimTypes.Name) ?? "Someone";

                var result = await _notificationService.CreateNotificationAsync(
                    recipientUserId: targetUserId,
                    type: "Message",
                    message: $"{senderName} sent you a message.",
                    relatedEntityId: messageId
                );

                return Ok(new { status = true, message = "Message test sent", data = result });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiErrorResponse(500, $"Failed to send message notification: {ex.Message}"));
            }
        }
    }
}
