using Roomify.GP.Core.Entities;
using Roomify.GP.Core.Repositories.Contract;
using Roomify.GP.Core.Service.Contract;
using Roomify.GP.Core.DTOs.PortfolioPost;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Roomify.GP.Service.Services
{
    public class PortfolioPostService : IPortfolioPostService
    {
        private readonly IPortfolioPostRepository _repo;

        public PortfolioPostService(IPortfolioPostRepository repo)
        {
            _repo = repo;
        }

        public async Task<IEnumerable<PortfolioPostResponseDto>> GetAllAsync()
        {
            var posts = await _repo.GetAllAsync();
            return posts.Select(post => MapToResponseDto(post));
        }

        public async Task<IEnumerable<PortfolioPostResponseDto>> GetByUserIdAsync(Guid userId)
        {
            var posts = await _repo.GetByUserIdAsync(userId);
            return posts.Select(post => MapToResponseDto(post));
        }

        public async Task<PortfolioPostResponseDto> GetByIdAsync(Guid id)
        {
            var post = await _repo.GetByIdAsync(id);
            return post == null ? null : MapToResponseDto(post);
        }

        public async Task AddAsync(Guid userId, PortfolioPost post)
        {
            await _repo.AddAsync(userId, post);
        }

        public async Task DeleteAsync(Guid id)
        {
            await _repo.DeleteAsync(id);
        }

        private PortfolioPostResponseDto MapToResponseDto(PortfolioPost post)
        {
            return new PortfolioPostResponseDto
            {
                Id = post.Id,
                ImagePath = post.ImagePath,
                Description = post.Description,
                CreatedAt = post.CreatedAt,
                ApplicationUserId = post.ApplicationUserId,
                OwnerUserName = post.ApplicationUser?.UserName,
                OwnerProfilePicture = post.ApplicationUser?.ProfilePicture
            };
        }
    }
}
