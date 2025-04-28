using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Roomify.GP.API.Hubs;
using Roomify.GP.Core.DTOs.ChatModel;
using Roomify.GP.Core.Service.Contract;
using Roomify.GP.Service;
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

        public ChatController(IHubContext<PrivateChatHub> hubContext, IMessageService messageService)
        {
            _hubContext = hubContext;
            _messageService = messageService;
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
    }
}
