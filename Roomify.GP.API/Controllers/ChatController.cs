using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Roomify.GP.API.Hubs;
using Roomify.GP.Service.Services;

namespace Roomify.GP.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ChatController : ControllerBase
    {
        private readonly IHubContext<PrivateChatHub> _hubContext;
        private readonly MessageService _messageService;

        public ChatController(IHubContext<PrivateChatHub> hubContext, MessageService messageService)
        {
            _hubContext = hubContext;
            _messageService = messageService;
        }

        [HttpPost("sendMessage")]
        public async Task<IActionResult> SendMessage(string senderId, string receiverId, string message)
        {
            // حفظ الرسالة في قاعدة البيانات
            await _messageService.SaveMessage(senderId, receiverId, message);

            // إرسال الرسالة عبر SignalR للمستقبل
            await _hubContext.Clients.User(receiverId).SendAsync("ReceiveMessage", message);
            return Ok("Message sent successfully.");
        }
    }
}
