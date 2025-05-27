using Roomify.GP.Core.Entities.AI.RoomImage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Roomify.GP.Core.Repositories.Contract
{
    public interface IPromptRepository
    {
        // Basic CRUD operations
        Task<Prompt> GetByIdAsync(Guid id);
        Task<List<Prompt>> GetAllAsync();
        Task<Prompt> AddAsync(Prompt entity);
        Task UpdateAsync(Prompt entity);
        Task DeleteAsync(Guid id);

        // Additional prompt-specific methods
        Task<List<Prompt>> GetByRoomImageIdAsync(Guid roomImageId);
        Task<List<Prompt>> GetByAIResultHistoryIdAsync(Guid aiResultHistoryId);
        Task<List<Prompt>> GetByUserIdAsync(Guid userId);
    }
}
