using Roomify.GP.Core.DTOs.ChatModel;
using System.Threading.Tasks;

namespace Roomify.GP.Core.Service.Contract
{
    public interface IMessageService
    {
        Task<List<MessageResponseDto>> GetMessagesAsync(Guid senderId, Guid receiverId);
        Task<bool> DeleteMessageAsync(Guid messageId, Guid currentUserId); // ✅ إضافة الميثود
        Task<string?> SaveMessageAsync(ChatModel chatModel);
        Task<MessageResponseDto> SaveMessageAndReturnAsync(ChatModel chatModel);
        Task<List<ChatPreviewDto>> GetAllChatsAsync(string userId);


    }
}
