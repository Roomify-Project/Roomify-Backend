using Roomify.GP.Core.DTOs.AI;
using Roomify.GP.Core.DTOs.Like;
using Roomify.GP.Core.DTOs.PortfolioPost;
using Roomify.GP.Core.Entities;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Roomify.GP.Core.Service.Contract
{
    public interface IPortfolioPostService
    {
        Task<(IEnumerable<PortfolioPostResponseDto> Posts, IEnumerable<SavedDesignResponseDto> Designs)> GetAllCombinedAsync();
        Task<IEnumerable<PortfolioPostResponseDto>> GetByUserIdAsync(Guid userId);
        Task<(string Type, object Data)> GetByIdCombinedAsync(Guid id);
        Task<PortfolioPostResponseDto> GetByIdAsync(Guid id);
        Task AddAsync(Guid userId, PortfolioPost post);
        Task DeleteAsync(Guid id);
        Task<LikeDto> AddLikeAsync(Guid id, bool isPost, Guid userId);
        Task RemoveLikeAsync(Guid id, bool isPost, Guid userId);
    }
    public interface ISavedDesignService
    {
        // Merged into PortfolioPostService for GetAll/GetById
        Task<LikeDto> AddLikeAsync(Guid designId, Guid userId);
        Task RemoveLikeAsync(Guid designId, Guid userId);
    }
}
