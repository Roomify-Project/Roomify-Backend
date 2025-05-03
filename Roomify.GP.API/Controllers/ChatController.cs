using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Roomify.GP.API.Hubs;
using Roomify.GP.Core.DTOs.ChatModel;
using Roomify.GP.Core.Service.Contract;
using Roomify.GP.Core.Services.Contract;
using System.Security.Claims;

namespace Roomify.GP.API.Controllers
{
    [Authorize(Roles = "NormalUser,InteriorDesigner")]
    [Route("api/[controller]")]
    [ApiController]
    public class ChatController : ControllerBase
    {
        private readonly IHubContext<PrivateChatHub> _hubContext;
        private readonly IMessageService _messageService;
        private readonly IUserConnectionService _userConnectionService;

        public ChatController(
            IHubContext<PrivateChatHub> hubContext,
            IMessageService messageService,
            IUserConnectionService userConnectionService)
        {
            _hubContext = hubContext;
            _messageService = messageService;
            _userConnectionService = userConnectionService;
        }

        [HttpPost("sendMessage")]
        [RequestSizeLimit(10 * 1024 * 1024)] // لو عايز تحدد الحجم المسموح للملف
        public async Task<IActionResult> SendMessage([FromForm] ChatModel chatModel)
        {
            var currentUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (currentUserId != chatModel.SenderId.ToString())
                return Unauthorized("You are not authorized to send this message.");

            if (chatModel.SenderId == Guid.Empty || chatModel.ReceiverId == Guid.Empty)
                return BadRequest("SenderId and ReceiverId must be valid.");

            var attachmentUrl = await _messageService.SaveMessageAsync(chatModel);

            await _hubContext.Clients.User(chatModel.ReceiverId.ToString())
                .SendAsync("ReceiveMessage", chatModel.Message, attachmentUrl);

            return Ok(new
            {
                message = "Message sent successfully.",
                attachmentUrl = attachmentUrl
            });
        }

        [HttpGet("getMessages/{userId}")]
        public async Task<IActionResult> GetMessages(Guid userId)
        {
            var currentUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!Guid.TryParse(currentUserId, out Guid parsedCurrentUserId))
                return Unauthorized("Invalid user.");

            var messages = await _messageService.GetMessagesAsync(parsedCurrentUserId, userId);
            return Ok(messages);
        }

        [HttpGet("isOnline/{userId}")]
        public async Task<IActionResult> IsUserOnline(Guid userId)
        {
            var isOnline = await _userConnectionService.IsUserOnlineAsync(userId);
            return Ok(new { userId, isOnline });
        }

        [HttpDelete("delete/{messageId}")]
        public async Task<IActionResult> DeleteMessage(Guid messageId)
        {
            var currentUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!Guid.TryParse(currentUserId, out Guid parsedUserId))
                return Unauthorized("Invalid user.");

            var result = await _messageService.DeleteMessageAsync(messageId, parsedUserId);
            if (!result)
                return NotFound("Message not found or you're not the sender.");

            return Ok("Message deleted successfully.");
        }
    }
}
