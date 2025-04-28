using Roomify.GP.Core.DTOs.ChatModel;
using System.Threading.Tasks;

namespace Roomify.GP.Core.Service.Contract
{
    public interface IMessageService
    {
        Task<List<MessageResponseDto>> GetMessagesAsync(Guid senderId, Guid receiverId);

        Task SaveMessage(ChatModel chatModel);
    }
}
