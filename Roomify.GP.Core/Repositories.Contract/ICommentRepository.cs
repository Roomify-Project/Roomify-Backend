using Roomify.GP.Core.Entities;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Roomify.GP.Core.Repositories.Contract
{
    public interface ICommentRepository
    {
        Task<IEnumerable<Comment>> GetAllByPostIdAsync(Guid postId);
        Task<Comment> GetByIdAsync(Guid id);
        Task<Comment> GetByIdWithOwnerCheckAsync(Guid id, Guid userId);
        Task AddAsync(Comment comment);
        Task UpdateAsync(Comment comment);
        Task DeleteAsync(Guid id);
        Task<bool> ExistsAsync(Guid id);
        Task<bool> IsAuthorizedUserAsync(Guid commentId, Guid userId);
        Task<bool> IsOwnerAsync(Guid commentId, Guid userId);
    }
}