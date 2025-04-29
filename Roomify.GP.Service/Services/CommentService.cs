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
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IMapper _mapper;
        private readonly ILogger<CommentService> _logger;

        public CommentService(
            ICommentRepository commentRepository,
            IPortfolioPostRepository postRepository,
            UserManager<ApplicationUser> userManager,
            IMapper mapper,
            ILogger<CommentService> logger)
        {
            _commentRepository = commentRepository;
            _postRepository = postRepository;
            _userManager = userManager;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<IEnumerable<CommentResponseDto>> GetAllByPostIdAsync(Guid postId)
        {
            var comments = await _commentRepository.GetAllByPostIdAsync(postId);
            return comments.Select(MapToResponseDto);
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
                _logger.LogInformation("Starting to add comment for user {UserId} on post {PostId}",
                    userId, commentDto.PortfolioPostId);

                var post = await _postRepository.GetByIdAsync(commentDto.PortfolioPostId);
                if (post == null)
                {
                    _logger.LogWarning("Portfolio post {PostId} not found", commentDto.PortfolioPostId);
                    throw new ApplicationException("Portfolio post not found.");
                }

                var user = await _userManager.FindByIdAsync(userId.ToString());
                if (user == null)
                {
                    _logger.LogWarning("User {UserId} not found", userId);
                    throw new ApplicationException("User not found.");
                }

                var comment = new Comment
                {
                    Content = commentDto.Content,
                    PortfolioPostId = commentDto.PortfolioPostId,
                    ApplicationUserId = userId,
                    CreatedAt = DateTime.UtcNow
                };

                await _commentRepository.AddAsync(comment);
                _logger.LogInformation("Comment {CommentId} successfully added", comment.Id);

                // Ensure we have navigation properties for response
                comment.ApplicationUser = user;
                comment.PortfolioPost = post;

                return MapToResponseDto(comment);
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
                _logger.LogInformation("Attempting to update comment {CommentId} by user {UserId}", id, userId);

                // Check if the user is the owner of the comment
                if (!await _commentRepository.IsOwnerAsync(id, userId))
                {
                    _logger.LogWarning("User {UserId} attempted to update comment {CommentId} but is not the owner",
                        userId, id);
                    throw new UnauthorizedAccessException("You are not authorized to update this comment.");
                }

                var comment = await _commentRepository.GetByIdAsync(id);
                if (comment == null)
                {
                    _logger.LogWarning("Comment {CommentId} not found for update", id);
                    throw new ApplicationException("Comment not found.");
                }

                comment.Content = commentDto.Content;
                comment.UpdatedAt = DateTime.UtcNow;
                await _commentRepository.UpdateAsync(comment);
                _logger.LogInformation("Comment {CommentId} successfully updated by user {UserId}", id, userId);

                // Get full user details if needed for response
                var user = await _userManager.FindByIdAsync(userId.ToString());
                if (user != null)
                {
                    comment.ApplicationUser = user;
                }

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
                _logger.LogInformation("Attempting to delete comment {CommentId} by user {UserId}", id, userId);

                // Check if the comment exists
                if (!await _commentRepository.ExistsAsync(id))
                {
                    _logger.LogWarning("Comment {CommentId} not found for deletion", id);
                    return false;
                }

                // Check if the user is the owner of the comment
                if (!await _commentRepository.IsOwnerAsync(id, userId))
                {
                    _logger.LogWarning("User {UserId} attempted to delete comment {CommentId} but is not the owner",
                        userId, id);
                    throw new UnauthorizedAccessException("You are not authorized to delete this comment.");
                }

                await _commentRepository.DeleteAsync(id);
                _logger.LogInformation("Comment {CommentId} successfully deleted by user {UserId}", id, userId);
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