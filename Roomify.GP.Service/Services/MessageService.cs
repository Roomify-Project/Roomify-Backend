using Roomify.GP.Repository.Data.Contexts;
using Roomify.GP.Core.Entities;
using System.Threading.Tasks;
using Roomify.GP.Core.DTOs.ChatModel;
using Roomify.GP.Core.Service.Contract;
using Microsoft.EntityFrameworkCore;

public class MessageService : IMessageService
{
    private readonly AppDbContext _context;
    private readonly ICloudinaryService _cloudinaryService;

    public MessageService(AppDbContext context, ICloudinaryService cloudinaryService)
    {
        _context = context;
        _cloudinaryService = cloudinaryService;
    }

    public async Task<string?> SaveMessageAsync(ChatModel chatmodel)
    {
        string? attachmentUrl = null;   

        // ✅ رفع الصورة لو موجودة
        if (chatmodel.File != null && chatmodel.File.Length > 0)
        {
            attachmentUrl = await _cloudinaryService.UploadImageAsync(chatmodel.File);
        }

        var newMessage = new Message
        {
            SenderId = chatmodel.SenderId,
            ReceiverId = chatmodel.ReceiverId,
            Content = chatmodel.Message ?? "",
            SentAt = DateTime.UtcNow,
            AttachmentUrl = attachmentUrl
        };

        _context.Messages.Add(newMessage);
        await _context.SaveChangesAsync();

        return attachmentUrl;
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
                SentAt = m.SentAt,
                AttachmentUrl = m.AttachmentUrl
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
    public async Task<MessageResponseDto> SaveMessageAndReturnAsync(ChatModel chatModel)
    {
        string? attachmentUrl = null;

        if (chatModel.File != null && chatModel.File.Length > 0)
        {
            attachmentUrl = await _cloudinaryService.UploadImageAsync(chatModel.File);
        }

        var newMessage = new Message
        {
            SenderId = chatModel.SenderId,
            ReceiverId = chatModel.ReceiverId,
            Content = chatModel.Message ?? "",
            SentAt = DateTime.UtcNow,
            AttachmentUrl = attachmentUrl
        };

        _context.Messages.Add(newMessage);
        await _context.SaveChangesAsync();

        return new MessageResponseDto
        {
            MessageId = newMessage.Id,
            SenderId = newMessage.SenderId,
            Content = newMessage.Content,
            SentAt = newMessage.SentAt,
            AttachmentUrl = newMessage.AttachmentUrl
        };
    }

}
