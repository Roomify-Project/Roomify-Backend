using Roomify.GP.Core.DTOs.PortfolioPost;
using Roomify.GP.Core.Entities;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Roomify.GP.Core.Service.Contract
{
    public interface IPortfolioPostService
    {
        Task<IEnumerable<PortfolioPostResponseDto>> GetAllAsync();
        Task<IEnumerable<PortfolioPostResponseDto>> GetByUserIdAsync(Guid userId);
        Task<PortfolioPostResponseDto> GetByIdAsync(Guid id);
        Task AddAsync(Guid userId, PortfolioPost post);
        Task DeleteAsync(Guid id);
    }
}
