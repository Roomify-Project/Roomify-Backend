using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Hosting;
using Roomify.GP.API.Errors;
using Roomify.GP.Core.DTOs.PortfolioPost;
using Roomify.GP.Core.Entities;
using Roomify.GP.Core.Entities.Identity;
using Roomify.GP.Core.Service.Contract;
using Roomify.GP.Service.Services;

namespace Roomify.GP.API.Controllers
{
    //[Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class PortfolioPostController : ControllerBase
    {
        private readonly IPortfolioPostService _portfolioPostService;
        private readonly ICloudinaryService _cloudinaryService;
        private readonly IMapper _mapper;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ILogger<PortfolioPostController> _logger;
        private readonly IRoomImageService _roomImageService;

        public PortfolioPostController(IPortfolioPostService portfolioPostService, ICloudinaryService cloudinaryService, IMapper mapper, UserManager<ApplicationUser> userManager, ILogger<PortfolioPostController> logger, IRoomImageService roomImageService)
        {
            _portfolioPostService = portfolioPostService;
            _cloudinaryService = cloudinaryService;
            _mapper = mapper;
            _userManager = userManager;
            _logger = logger;
            _roomImageService = roomImageService;
        }

        [ProducesResponseType(typeof(IActionResult), StatusCodes.Status200OK)]
        [HttpGet]      // GET api/portfolio
        public async Task<IActionResult> GetAll()
        {
            try
            {
                var (posts, designs) = await _portfolioPostService.GetAllCombinedAsync();
                return Ok(new { PortfolioPosts = posts, SavedDesigns = designs });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving combined portfolio");
                return StatusCode(500, new ApiErrorResponse(500, "Failed to retrieve data"));
            }
        }


        [ProducesResponseType(typeof(IActionResult), StatusCodes.Status200OK)]
        [HttpGet("{id}")]   // GET api/portfolio/{id}
        public async Task<IActionResult> GetById(Guid id)
        {
            try
            {
                var result = await _portfolioPostService.GetByIdCombinedAsync(id);
                return Ok(new { Type = result.Type, Data = result.Data });
            }
            catch (KeyNotFoundException)
            {
                return NotFound(new ApiErrorResponse(404, "Item not found"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving item {Id}", id);
                return StatusCode(500, new ApiErrorResponse(500, "Failed to retrieve item"));
            }
        }


        [ProducesResponseType(typeof(IActionResult), StatusCodes.Status200OK)]
        [HttpGet("by-user/{userId}")]
        public async Task<IActionResult> GetByUser(Guid userId)
        {
            var posts = await _portfolioPostService.GetByUserIdAsync(userId);
            var response = _mapper.Map<List<PortfolioPostResponseDto>>(posts);
            return Ok(response);
        }


        [ProducesResponseType(typeof(IActionResult), StatusCodes.Status200OK)]
        [HttpPost("upload/{userId}")]      // POST api/portfolio/upload/{userId}
        public async Task<IActionResult> Upload(Guid userId, [FromForm] PortfolioPostDto portfolioPostDto)
        {
            if (userId == Guid.Empty)
                return BadRequest(new ApiErrorResponse(400, "Invalid User ID"));

            var user = await _userManager.FindByIdAsync(userId.ToString());
            if (user == null)
                return NotFound(new ApiErrorResponse(404, "User not found."));

            if (portfolioPostDto.ImageFile == null || portfolioPostDto.ImageFile.Length == 0)
                return BadRequest(new ApiErrorResponse(400, "Image is required."));

            try
            {
                var imageUrl = await _cloudinaryService.UploadImageAsync(portfolioPostDto.ImageFile);

                var post = _mapper.Map<PortfolioPost>(portfolioPostDto);
                post.Id = Guid.NewGuid();
                post.ImagePath = imageUrl;
                post.CreatedAt = DateTime.UtcNow;
                post.ApplicationUser = user;
                post.ApplicationUserId = userId;

                await _portfolioPostService.AddAsync(userId, post);
                _logger.LogInformation("Post uploaded successfully for user {UserId}", userId);

                return Ok(new
                {
                    message = "Post uploaded successfully.",
                    ImagePath = imageUrl,
                    id = post.Id,
                    user = new
                    {
                        id = user.Id,
                        userName = user.UserName,
                        profilePicture = user.ProfilePicture
                    }
                });
            }
            catch (ApplicationException ex)
            {
                return BadRequest(new ApiErrorResponse(400, ex.Message));
            }
            catch (Exception)
            {
                return StatusCode(500, new ApiErrorResponse(500, "Unexpected server error during image upload."));
            }
        }


        [ProducesResponseType(typeof(IActionResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
        [HttpDelete("{id}")]     // DELETE api/portfolio/{id}
        public async Task<IActionResult> Delete(Guid id)
        {
            if (id == Guid.Empty)
                return BadRequest(new ApiErrorResponse(400));

            var post = await _portfolioPostService.GetByIdAsync(id);
            if (post == null)
                return NotFound(new ApiErrorResponse(404));

            try
            {
                await _cloudinaryService.DeleteImageAsync(post.ImagePath);
            }
            catch (ApplicationException ex)
            {
                // Log or continue, image deletion failure shouldn't block post deletion
                return Ok(new { message = "Post deleted, but image deletion failed", details = ex.Message });
            }

            await _portfolioPostService.DeleteAsync(id);

            return Ok(new { message = "Post and image deleted successfully." });
        }



        // POST api/portfolio/{id}/likes?isPost=true&userId={userId}
        [HttpPost("{id}/likes")]
        public async Task<IActionResult> Like(Guid id, [FromQuery] bool isPost, [FromQuery] Guid userId)
        {
            try
            {
                var like = await _portfolioPostService.AddLikeAsync(id, isPost, userId);
                return Ok(like);
            }
            catch (ApplicationException ex)
            {
                return BadRequest(new ApiErrorResponse(400, ex.Message));
            }
        }

        // DELETE api/portfolio/{id}/likes?isPost=true&userId={userId}
        [HttpDelete("{id}/likes")]
        public async Task<IActionResult> Unlike(Guid id, [FromQuery] bool isPost, [FromQuery] Guid userId)
        {
            await _portfolioPostService.RemoveLikeAsync(id, isPost, userId);
            return NoContent();
        }
    }
}
