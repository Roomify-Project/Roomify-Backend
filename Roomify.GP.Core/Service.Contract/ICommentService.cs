using Roomify.GP.Core.DTOs.Comment;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Roomify.GP.Core.Service.Contract
{
    public interface ICommentService
    {
        Task<IEnumerable<CommentResponseDto>> GetAllAsync(Guid? postId, Guid? designId);
        Task<CommentResponseDto> GetByIdAsync(Guid id);
        Task<CommentResponseDto> AddAsync(Guid userId, CommentCreateDto commentDto);
        Task<CommentResponseDto> UpdateAsync(Guid id, Guid userId, CommentUpdateDto commentDto);
        Task<bool> DeleteAsync(Guid id, Guid userId);
    }
}
