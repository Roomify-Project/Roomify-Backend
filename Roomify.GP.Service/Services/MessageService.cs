using Roomify.GP.Repository.Data.Contexts;

public class MessageService
{
    private readonly AppDbContext _context;

    public MessageService(AppDbContext context)
    {
        _context = context;
    }

    public async Task SaveMessage(Guid senderId, Guid receiverId, string message)
    {
        var newMessage = new Message
        {
            SenderId = senderId, // استخدام string
            ReceiverId = receiverId, // استخدام string
            Content = message,
            SentAt = DateTime.UtcNow
        };

        _context.Messages.Add(newMessage);
        await _context.SaveChangesAsync();
    }
}
