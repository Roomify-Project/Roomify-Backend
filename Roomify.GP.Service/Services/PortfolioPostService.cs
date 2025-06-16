using Roomify.GP.Core.Entities;
using Roomify.GP.Core.Repositories.Contract;
using Roomify.GP.Core.Service.Contract;
using Roomify.GP.Core.DTOs.PortfolioPost;
using Roomify.GP.Core.DTOs.AI;
using Roomify.GP.Core.DTOs.Like;
using Roomify.GP.Core.DTOs.Comment;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;

namespace Roomify.GP.Service.Services
{
    public class PortfolioPostService : IPortfolioPostService
    {
        private readonly IPortfolioPostRepository _portfolioPostRepository;
        private readonly ISavedDesignRepository _savedDesignRepository;
        private readonly IMapper _mapper;

        public PortfolioPostService(
            IPortfolioPostRepository portfolioPostRepository,
            ISavedDesignRepository savedDesignRepository,
            IMapper mapper)
        {
            _portfolioPostRepository = portfolioPostRepository;
            _savedDesignRepository = savedDesignRepository;
            _mapper = mapper;
        }

        public async Task<(IEnumerable<PortfolioPostResponseDto>, IEnumerable<SavedDesignResponseDto>)> GetAllCombinedAsync(Guid userId)
        {
            var posts = await _portfolioPostRepository.GetAllAsync();
            var designs = await _savedDesignRepository.GetAllWithUserInfoAsync();

            var mappedPosts = new List<PortfolioPostResponseDto>();
            foreach (var post in posts)
            {
                var dto = await MapToResponseDtoAsync(post, userId);
                mappedPosts.Add(dto);
            }

            var mappedDesigns = designs.Select(d => _mapper.Map<SavedDesignResponseDto>(d));
            return (mappedPosts, mappedDesigns);
        }

        public async Task<IEnumerable<PortfolioPostResponseDto>> GetByUserIdAsync(Guid userId)
        {
            var posts = await _portfolioPostRepository.GetByUserIdAsync(userId);
            var responseList = new List<PortfolioPostResponseDto>();

            foreach (var post in posts)
            {
                var dto = await MapToResponseDtoAsync(post, userId);
                responseList.Add(dto);
            }

            return responseList;
        }

        public async Task<(string Type, object Data)> GetByIdCombinedAsync(Guid id, Guid userId)
        {
            var post = await _portfolioPostRepository.GetByIdAsync(id);
            if (post != null)
            {
                var dto = await MapToResponseDtoAsync(post, userId);
                return ("Post", dto);
            }

            var design = await _savedDesignRepository.GetByIdWithUserInfoAsync(id);
            if (design != null)
            {
                var dto = _mapper.Map<SavedDesignResponseDto>(design);
                dto.IsLiked = userId != Guid.Empty && await _savedDesignRepository.LikeExistsAsync(id, userId);
                return ("Design", dto);
            }

            throw new KeyNotFoundException("Item not found.");
        }

        public async Task<PortfolioPostResponseDto> GetByIdAsync(Guid id)
        {
            var post = await _portfolioPostRepository.GetByIdAsync(id);
            return post == null ? null : await MapToResponseDtoAsync(post, Guid.Empty);
        }

        public async Task AddAsync(Guid userId, PortfolioPost post)
        {
            await _portfolioPostRepository.AddAsync(userId, post);
        }

        public async Task DeleteAsync(Guid id)
        {
            await _portfolioPostRepository.DeleteAsync(id);
        }

        public async Task<LikeDto> AddLikeAsync(Guid id, bool isPost, Guid userId)
        {
            var exists = isPost
                ? await _portfolioPostRepository.LikeExistsAsync(id, userId)
                : await _savedDesignRepository.LikeExistsAsync(id, userId);

            if (exists)
                throw new ApplicationException("Already liked");

            var like = new Like
            {
                ApplicationUserId = userId,
                PortfolioPostId = isPost ? id : (Guid?)null,
                SavedDesignId = isPost ? (Guid?)null : id
            };

            if (isPost)
                await _portfolioPostRepository.AddLikeAsync(like);
            else
                await _savedDesignRepository.AddLikeAsync(like);

            return _mapper.Map<LikeDto>(like);
        }

        public async Task RemoveLikeAsync(Guid id, bool isPost, Guid userId)
        {
            if (isPost)
                await _portfolioPostRepository.RemoveLikeAsync(id, userId);
            else
                await _savedDesignRepository.RemoveLikeAsync(id, userId);
        }

        private async Task<PortfolioPostResponseDto> MapToResponseDtoAsync(PortfolioPost post, Guid userId)
        {
            return new PortfolioPostResponseDto
            {
                Id = post.Id,
                ImagePath = post.ImagePath,
                Description = post.Description,
                CreatedAt = post.CreatedAt,
                UserId = post.ApplicationUserId,
                UserName = post.ApplicationUser?.UserName,
                UserProfilePicture = post.ApplicationUser?.ProfilePicture,
                IsLiked = userId != Guid.Empty && await _portfolioPostRepository.LikeExistsAsync(post.Id, userId),
                LikesCount = post.Likes?.Count ?? 0,
                Comments = post.Comments?.Select(c => new CommentResponseDto
                {
                    Id = c.Id,
                    Content = c.Content,
                    CreatedAt = c.CreatedAt,
                    UpdatedAt = c.UpdatedAt,
                    UserId = c.ApplicationUserId,
                    UserName = c.ApplicationUser?.UserName,
                    UserProfilePicture = c.ApplicationUser?.ProfilePicture,
                    PortfolioPostId = c.PortfolioPostId
                }).ToList()
            };
        }
    }
}
