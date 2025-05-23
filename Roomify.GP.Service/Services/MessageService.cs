    using Roomify.GP.Repository.Data.Contexts;
using Roomify.GP.Core.Entities;
using System.Threading.Tasks;
using Roomify.GP.Core.DTOs.ChatModel;
using Roomify.GP.Core.Service.Contract;
using Roomify.GP.Core.Services.Contract;
using Roomify.GP.Core.Repositories.Contract;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;

public class MessageService : IMessageService
{
    private readonly AppDbContext _context;
    private readonly INotificationService _notificationService;
    private readonly IUserRepository _userRepository;

    public MessageService(
        AppDbContext context,
        INotificationService notificationService,
        IUserRepository userRepository)
    {
        _context = context;
        _notificationService = notificationService;
        _userRepository = userRepository;
    }

    public async Task SaveMessage(ChatModel chatmodel)
    {
        // إنشاء وحفظ الرسالة
        var newMessage = new Message
        {
            SenderId = chatmodel.SenderId,
            ReceiverId = chatmodel.ReceiverId,
            Content = chatmodel.Message,
            SentAt = DateTime.UtcNow
        };

        _context.Messages.Add(newMessage);
        await _context.SaveChangesAsync();

        // بعد نجاح حفظ الرسالة، إرسال إشعار للمستلم
        var sender = await _userRepository.GetUserByIdAsync(chatmodel.SenderId);
        if (sender != null)
        {
            await _notificationService.CreateMessageNotificationAsync(
                chatmodel.ReceiverId,
                chatmodel.SenderId,
                sender.UserName);
        }
    }

    public async Task<List<MessageResponseDto>> GetMessagesAsync(Guid senderId, Guid receiverId)
    {
        var messages = await _context.Messages
            .Where(m => (m.SenderId == senderId && m.ReceiverId == receiverId)
                     || (m.SenderId == receiverId && m.ReceiverId == senderId))
            .OrderBy(m => m.SentAt)
            .Select(m => new MessageResponseDto
            {
                MessageId = m.Id,
                SenderId = m.SenderId,
                Content = m.Content,
                SentAt = m.SentAt
            })
            .ToListAsync();

        return messages;
    }

    public async Task<bool> DeleteMessageAsync(Guid messageId, Guid currentUserId)
    {
        var message = await _context.Messages.FindAsync(messageId);
        if (message == null || message.SenderId != currentUserId)
            return false;

        message.Content = "message is deleted";
        message.IsDeleted = true;
        await _context.SaveChangesAsync();
        return true;
    }
}