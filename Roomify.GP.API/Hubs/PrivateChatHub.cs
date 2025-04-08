using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Roomify.GP.Service.Services;

namespace Roomify.GP.API.Hubs
{
  
    public class PrivateChatHub : Hub
    {
        private readonly MessageService _messageService;

        // حقن الـ MessageService
        public PrivateChatHub(MessageService messageService)
        {
            _messageService = messageService;
        }

        // إرسال رسالة خاصة لمستخدم معين
        public async Task SendMessageToUser(string senderId, string receiverId, string message)
        {
            // حفظ الرسالة في قاعدة البيانات
            await _messageService.SaveMessage(senderId, receiverId, message);

            // إرسال الرسالة للمستخدم المحدد
            await Clients.User(receiverId).SendAsync("ReceiveMessage", message);
        }

        // إرسال رسالة لجميع المتصلين (اختياري لو كنت عايز الرسائل تكون عامة)
        public async Task SendMessageToAll(string message)
        {
            await Clients.All.SendAsync("ReceiveMessage", message);
        }
    }
}
