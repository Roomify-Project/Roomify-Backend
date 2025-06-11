using Roomify.GP.Core.Entities;
using Roomify.GP.Core.Entities.AI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Roomify.GP.Core.Repositories.Contract
{
    public interface ISavedDesignRepository
    {
        //Task<List<SavedDesign>> GetByUserIdAsync(Guid userId);
        Task<List<SavedDesign>> GetAllWithUserInfoAsync();
        Task<List<SavedDesign>> GetByUserIdWithUserInfoAsync(Guid userId);
        Task<SavedDesign> GetByIdWithUserInfoAsync(Guid id);
        Task AddAsync(SavedDesign entity);
        Task DeleteAsync(Guid id);

        // Comments
        Task<IEnumerable<Comment>> GetAllCommentsAsync(Guid designId);

        // Likes
        Task<bool> LikeExistsAsync(Guid designId, Guid userId);
        Task AddLikeAsync(Like like);
        Task RemoveLikeAsync(Guid designId, Guid userId);
        Task<int> GetLikesCountAsync(Guid designId);

    }
}
