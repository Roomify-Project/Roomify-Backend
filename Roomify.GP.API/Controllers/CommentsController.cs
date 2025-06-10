using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Roomify.GP.API.Errors;
using Roomify.GP.Core.DTOs.Comment;
using Roomify.GP.Core.Entities.Identity;
using Roomify.GP.Core.Service.Contract;
using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace Roomify.GP.API.Controllers
{
    //[Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class CommentsController : ControllerBase
    {
        private readonly ICommentService _commentService;
        private readonly ILogger<CommentsController> _logger;
        private readonly UserManager<ApplicationUser> _userManager;

        public CommentsController(ICommentService commentService, ILogger<CommentsController> logger, UserManager<ApplicationUser> userManager)
        {
            _commentService = commentService;
            _logger = logger;
            _userManager = userManager;
        }

        
        [HttpGet]   // GET api/comments?postId={postId}&designId={designId}
        public async Task<IActionResult> GetAll([FromQuery] Guid? postId, [FromQuery] Guid? designId)
        {
            try
            {
                var comments = await _commentService.GetAllAsync(postId, designId);
                return Ok(comments);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving comments for postId: {PostId}, designId: {DesignId}", postId, designId);
                return StatusCode(500, new ApiErrorResponse(500, "Failed to retrieve comments"));
            }
        }


        [HttpGet("{id}")]
        public async Task<IActionResult> GetCommentById(Guid id)
        {
            try
            {
                var comment = await _commentService.GetByIdAsync(id);
                if (comment == null)
                    return NotFound(new ApiErrorResponse(404, "Comment not found"));

                return Ok(comment);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving comment {CommentId}", id);
                return StatusCode(500, new ApiErrorResponse(500, "Failed to retrieve comment"));
            }
        }


        [HttpPost]      // POST api/comments?userId={userId} 
        public async Task<IActionResult> CreateComment([FromQuery] Guid userId, [FromBody] CommentCreateDto commentDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(new ApiValidationErrorResponse { Errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage) });

            try
            {
                if (userId == Guid.Empty)
                    return BadRequest(new ApiErrorResponse(400, "Invalid User ID."));

                var user = await _userManager.FindByIdAsync(userId.ToString());
                if (user == null)
                    return NotFound(new ApiErrorResponse(404, "User not found."));

                var created = await _commentService.AddAsync(userId,commentDto);
                return CreatedAtAction(nameof(GetAll), new { id = created.Id, postId = commentDto.PortfolioPostId, designId = commentDto.SavedDesignId }, created);
            }
            catch (ApplicationException ex)
            {
                _logger.LogError(ex, "Error creating comment: {Message}", ex.Message);
                return BadRequest(new ApiErrorResponse(400, ex.Message));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error creating comment: {Message}", ex.Message);
                return StatusCode(500, new ApiErrorResponse(500, "Failed to create comment"));
            }
        }


        [HttpPut("{id}")]      // PUT api/comments/{id}?userId={userId}
        public async Task<IActionResult> UpdateComment(Guid id, [FromQuery] Guid userId, [FromBody] CommentUpdateDto commentDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(new ApiValidationErrorResponse { Errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage) });

            try
            {
                if (userId == Guid.Empty)
                    return BadRequest(new ApiErrorResponse(400, "Invalid User ID."));

                var user = await _userManager.FindByIdAsync(userId.ToString());
                if (user == null)
                    return NotFound(new ApiErrorResponse(404, "User not found."));

                _logger.LogInformation("Updating comment {CommentId} by user {UserId}", id, userId);
                var updated = await _commentService.UpdateAsync(id, userId, commentDto);
                return Ok(updated);
            }
            catch (ApplicationException ex)
            {
                _logger.LogError(ex, "Error updating comment {CommentId}: {Message}", id, ex.Message);
                return BadRequest(new ApiErrorResponse(400, ex.Message));
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogWarning(ex, "Unauthorized attempt to update comment {CommentId}: {Message}", id, ex.Message);
                return Unauthorized(new ApiErrorResponse(401, ex.Message));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error updating comment {CommentId}: {Message}", id, ex.Message);
                return StatusCode(500, new ApiErrorResponse(500, "Failed to update comment"));
            }
        }


        [HttpDelete("{id}")]       // DELETE api/comments/{id}?userId={userId}
        public async Task<IActionResult> DeleteComment(Guid id, [FromQuery] Guid userId)
        {
            try
            {
                if (userId == Guid.Empty)
                    return BadRequest(new ApiErrorResponse(400, "Invalid User ID."));

                var user = await _userManager.FindByIdAsync(userId.ToString());
                if (user == null)
                    return NotFound(new ApiErrorResponse(404, "User not found."));

                _logger.LogInformation("Deleting comment {CommentId} by user {UserId}", id, userId);
                var success = await _commentService.DeleteAsync(id, userId);
                if (!success)
                    return NotFound(new ApiErrorResponse(404, "Comment not found."));

                return Ok(new { message = "Comment deleted" });
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogWarning(ex, "Unauthorized attempt to delete comment {CommentId}: {Message}", id, ex.Message);
                return Unauthorized(new ApiErrorResponse(401, ex.Message));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error deleting comment {CommentId}: {Message}", id, ex.Message);
                return StatusCode(500, new ApiErrorResponse(500, "Failed to delete comment"));
            }
        }
    }
}