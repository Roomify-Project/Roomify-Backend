using AutoMapper;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Roomify.GP.Core.DTOs.Comment;
using Roomify.GP.Core.Entities;
using Roomify.GP.Core.Entities.Identity;
using Roomify.GP.Core.Repositories.Contract;
using Roomify.GP.Core.Service.Contract;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Roomify.GP.Service.Services
{
    public class CommentService : ICommentService
    {
        private readonly ICommentRepository _commentRepository;
        private readonly IPortfolioPostRepository _postRepository;
        private readonly ISavedDesignRepository _savedDesignRepository;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IMapper _mapper;
        private readonly ILogger<CommentService> _logger;
        private readonly INotificationService _notificationService;

        public CommentService(
            ICommentRepository commentRepository,
            IPortfolioPostRepository postRepository,
            ISavedDesignRepository savedDesignRepository,
            UserManager<ApplicationUser> userManager,
            IMapper mapper,
            ILogger<CommentService> logger,
            INotificationService notificationService)
        {
            _commentRepository = commentRepository;
            _postRepository = postRepository;
            _savedDesignRepository = savedDesignRepository;
            _userManager = userManager;
            _mapper = mapper;
            _logger = logger;
            _notificationService = notificationService;
        }

        public async Task<IEnumerable<CommentResponseDto>> GetAllAsync(Guid? postId, Guid? designId)
        {
            if (postId.HasValue)
                return (await _commentRepository.GetAllByPostIdAsync(postId.Value))
                       .Select(c => _mapper.Map<CommentResponseDto>(c));
            if (designId.HasValue)
                return (await _commentRepository.GetAllBySavedDesignIdAsync(designId.Value))
                       .Select(c => _mapper.Map<CommentResponseDto>(c));
            throw new ArgumentException("postId or designId must be provided");
        }

        public async Task<CommentResponseDto> GetByIdAsync(Guid id)
        {
            var comment = await _commentRepository.GetByIdAsync(id);
            return comment != null ? MapToResponseDto(comment) : null;
        }

        public async Task<CommentResponseDto> AddAsync(Guid userId, CommentCreateDto commentDto)
        {
            try
            {
                if (!commentDto.PortfolioPostId.HasValue && !commentDto.SavedDesignId.HasValue)
                    throw new ArgumentException("Provide postId or designId");

                var entity = _mapper.Map<Comment>(commentDto);
                entity.ApplicationUserId = userId; // ✅ حل المشكلة

                if (commentDto.PortfolioPostId.HasValue)
                    _ = await _postRepository.GetByIdAsync(commentDto.PortfolioPostId.Value)
                        ?? throw new ApplicationException("Post not found");
                else
                    _ = await _savedDesignRepository.GetByIdWithUserInfoAsync(commentDto.SavedDesignId.Value)
                        ?? throw new ApplicationException("Design not found");

                await _commentRepository.AddAsync(entity);
                var full = await _commentRepository.GetByIdAsync(entity.Id);
                return _mapper.Map<CommentResponseDto>(full);
            }
            catch (ApplicationException ex)
            {
                _logger.LogError(ex, "Error adding comment: {Message}", ex.Message);
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error adding comment: {Message}", ex.Message);
                throw new ApplicationException($"Failed to add comment: {ex.Message}", ex);
            }
        }

        public async Task<CommentResponseDto> UpdateAsync(Guid id, Guid userId, CommentUpdateDto commentDto)
        {
            try
            {
                if (!await _commentRepository.IsAuthorizedUserAsync(id, userId))
                    throw new UnauthorizedAccessException("You are not authorized to update this comment.");

                var comment = await _commentRepository.GetByIdAsync(id);
                if (comment == null)
                    throw new ApplicationException("Comment not found.");

                comment.Content = commentDto.Content;
                comment.UpdatedAt = DateTime.UtcNow;
                await _commentRepository.UpdateAsync(comment);

                var user = await _userManager.FindByIdAsync(userId.ToString());
                if (user != null)
                    comment.ApplicationUser = user;

                return MapToResponseDto(comment);
            }
            catch (ApplicationException ex)
            {
                _logger.LogError(ex, "Application error updating comment {CommentId}: {Message}", id, ex.Message);
                throw;
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogWarning(ex, "Authorization error updating comment {CommentId}: {Message}", id, ex.Message);
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error updating comment {CommentId}: {Message}", id, ex.Message);
                throw new ApplicationException($"Failed to update comment: {ex.Message}", ex);
            }
        }

        public async Task<bool> DeleteAsync(Guid id, Guid userId)
        {
            try
            {
                if (!await _commentRepository.ExistsAsync(id))
                    return false;

                if (!await _commentRepository.IsAuthorizedUserAsync(id, userId))
                    throw new UnauthorizedAccessException("You are not authorized to delete this comment.");

                await _commentRepository.DeleteAsync(id);
                return true;
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogWarning(ex, "Authorization error deleting comment {CommentId}: {Message}", id, ex.Message);
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error deleting comment {CommentId}: {Message}", id, ex.Message);
                throw new ApplicationException($"Failed to delete comment: {ex.Message}", ex);
            }
        }

        private CommentResponseDto MapToResponseDto(Comment comment)
        {
            if (comment == null) return null;

            return new CommentResponseDto
            {
                Id = comment.Id,
                Content = comment.Content,
                CreatedAt = comment.CreatedAt,
                UpdatedAt = comment.UpdatedAt,
                UserId = comment.ApplicationUserId,
                UserName = comment.ApplicationUser?.UserName,
                UserProfilePicture = comment.ApplicationUser?.ProfilePicture,
                PortfolioPostId = comment.PortfolioPostId
            };
        }
    }
}
