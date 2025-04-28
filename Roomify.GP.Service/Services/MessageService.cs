using Roomify.GP.Repository.Data.Contexts;
using Roomify.GP.Core.Entities; 
using System.Threading.Tasks;
using Roomify.GP.Core.DTOs.ChatModel;
using Roomify.GP.Core.Service.Contract;

public class MessageService :IMessageService
{
    private readonly AppDbContext _context;

    public MessageService(AppDbContext context)
    {
        _context = context;
    }

    public async Task SaveMessage(ChatModel chatmodel)
    {
        var newMessage = new Message
        {
            SenderId = chatmodel.SenderId,
            ReceiverId = chatmodel.ReceiverId,
            Content = chatmodel.Message,
            SentAt = DateTime.UtcNow
        };

        _context.Messages.Add(newMessage);
        await _context.SaveChangesAsync();
    }
}
