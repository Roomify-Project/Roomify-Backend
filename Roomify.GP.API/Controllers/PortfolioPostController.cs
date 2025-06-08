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

namespace Roomify.GP.API.Controllers
{
    //[Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class PortfolioPostController : ControllerBase
    {
        private readonly IPortfolioPostService _service;
        private readonly ICloudinaryService _cloudinaryService;
        private readonly IMapper _mapper;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ILogger<PortfolioPostController> _logger;
        private readonly IRoomImageService _roomImageService;

        public PortfolioPostController(IPortfolioPostService service, ICloudinaryService cloudinaryService, IMapper mapper, UserManager<ApplicationUser> userManager, ILogger<PortfolioPostController> logger, IRoomImageService roomImageService)
        {
            _service = service;
            _cloudinaryService = cloudinaryService;
            _mapper = mapper;
            _userManager = userManager;
            _logger = logger;
            _roomImageService = roomImageService;
        }

        [ProducesResponseType(typeof(IActionResult), StatusCodes.Status200OK)]
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            try
            {
                // Get all portfolio posts
                var posts = await _service.GetAllAsync();
                var portfolioResponse = _mapper.Map<List<PortfolioPostResponseDto>>(posts);

                // Get all saved designs with user info
                var savedDesigns = await _roomImageService.GetAllSavedDesignsWithUserInfoAsync();

                // Create combined response
                var combinedResponse = new
                {
                    PortfolioPosts = portfolioResponse,
                    SavedDesigns = savedDesigns.Select(sd => new
                    {
                        Id = sd.Id,
                        GeneratedImageUrl = sd.GeneratedImageUrl,
                        SavedAt = sd.SavedAt,
                        UserId = sd.ApplicationUserId,
                        UserFullName = sd.ApplicationUser?.FullName,
                        UserProfilePicture = sd.ApplicationUser?.ProfilePicture
                    })
                };

                return Ok(combinedResponse);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving all posts and saved designs");
                return StatusCode(500, new ApiErrorResponse(500, "An error occurred while retrieving data"));
            }
        }


        [ProducesResponseType(typeof(IActionResult), StatusCodes.Status200OK)]
        [HttpGet("by-user/{userId}")]
        public async Task<IActionResult> GetByUser(Guid userId)
        {
            var posts = await _service.GetByUserIdAsync(userId);
            var response = _mapper.Map<List<PortfolioPostResponseDto>>(posts);
            return Ok(response);
        }


        [ProducesResponseType(typeof(IActionResult), StatusCodes.Status200OK)]
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            try
            {
                // First try to get as a portfolio post
                var post = await _service.GetByIdAsync(id);
                if (post != null)
                {
                    var portfolioResponse = _mapper.Map<PortfolioPostResponseDto>(post);
                    return Ok(new { Type = "PortfolioPost", Data = portfolioResponse });
                }

                // If not found as portfolio post, try to get as saved design
                var savedDesign = await _roomImageService.GetSavedDesignByIdWithUserInfoAsync(id);
                if (savedDesign != null)
                {
                    var savedDesignResponse = new
                    {
                        Id = savedDesign.Id,
                        GeneratedImageUrl = savedDesign.GeneratedImageUrl,
                        SavedAt = savedDesign.SavedAt,
                        UserId = savedDesign.ApplicationUserId,
                        UserFullName = savedDesign.ApplicationUser?.FullName,
                        UserProfilePicture = savedDesign.ApplicationUser?.ProfilePicture
                    };
                    return Ok(new { Type = "SavedDesign", Data = savedDesignResponse });
                }

                return NotFound(new ApiErrorResponse(404, "Item not found"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving item by ID: {Id}", id);
                return StatusCode(500, new ApiErrorResponse(500, "An error occurred while retrieving the item"));
            }
        }


        [ProducesResponseType(typeof(IActionResult), StatusCodes.Status200OK)]
        [HttpPost("upload/{userId}")]
        //[Authorize(Roles = Roles.InteriorDesigner)]
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

                await _service.AddAsync(userId, post);
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
        [HttpDelete("{id}")]
        //[Authorize(Roles = Roles.InteriorDesigner)]
        public async Task<IActionResult> Delete(Guid id)
        {
            if (id == Guid.Empty)
                return BadRequest(new ApiErrorResponse(400));

            var post = await _service.GetByIdAsync(id);
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

            await _service.DeleteAsync(id);

            return Ok(new { message = "Post and image deleted successfully." });
        }
    }
}
