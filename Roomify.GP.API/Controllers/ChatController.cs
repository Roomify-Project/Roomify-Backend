using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Roomify.GP.API.Hubs;
using Roomify.GP.Core.DTOs.ChatModel;
using Roomify.GP.Core.Service.Contract;
using Roomify.GP.Core.Services.Contract;
using Roomify.GP.Service;
using Roomify.GP.Service.Services;
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
        public ChatController(IHubContext<PrivateChatHub> hubContext, IMessageService messageService , IUserConnectionService userConnectionService)
        {
            _hubContext = hubContext;
            _messageService = messageService;
            _userConnectionService = userConnectionService;
        }

        [HttpPost("sendMessage")]
        public async Task<IActionResult> SendMessage([FromBody] ChatModel chatModel)
        {
            // استخراج الـ UserId من الـ Token
            var currentUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (currentUserId != chatModel.SenderId.ToString())
                return Unauthorized("You are not authorized to send this message.");


            if (string.IsNullOrWhiteSpace(chatModel.Message))
            {
                return BadRequest("Message is required.");
            }

            if (chatModel.SenderId == Guid.Empty || chatModel.ReceiverId == Guid.Empty)
            {
                return BadRequest("SenderId and ReceiverId must be valid.");
            }

            // حفظ الرسالة في قاعدة البيانات
            await _messageService.SaveMessage(chatModel);

            // إرسال الرسالة باستخدام SignalR
            await _hubContext.Clients.User(chatModel.ReceiverId.ToString())
                .SendAsync("ReceiveMessage", chatModel.Message);

            return Ok("Message sent successfully.");
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
