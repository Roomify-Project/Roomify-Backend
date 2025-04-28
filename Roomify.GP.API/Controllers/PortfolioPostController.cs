using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Roomify.GP.API.Errors;
using Roomify.GP.Core.DTOs.PortfolioPost;
using Roomify.GP.Core.Entities;
using Roomify.GP.Core.Entities.Identity;
using Roomify.GP.Core.Service.Contract;

namespace Roomify.GP.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PortfolioPostController : ControllerBase
    {
        private readonly IPortfolioPostService _service;
        private readonly ICloudinaryService _cloudinaryService;
        private readonly IMapper _mapper;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ILogger<PortfolioPostController> _logger;

        public PortfolioPostController(IPortfolioPostService service, ICloudinaryService cloudinaryService, IMapper mapper, UserManager<ApplicationUser> userManager, ILogger<PortfolioPostController> logger)
        {
            _service = service;
            _cloudinaryService = cloudinaryService;
            _mapper = mapper;
            _userManager = userManager;
            _logger = logger;
        }

        [ProducesResponseType(typeof(IActionResult), StatusCodes.Status200OK)]
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var posts = await _service.GetAllAsync();
            var response = _mapper.Map<List<PortfolioPostResponseDto>>(posts);
            return Ok(response);
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
            var post = await _service.GetByIdAsync(id);
            if (post == null)
                return NotFound(new ApiErrorResponse(404, "The Post Doesn't Exist"));

            var response = _mapper.Map<PortfolioPostResponseDto>(post);
            return Ok(response);
        }

        [ProducesResponseType(typeof(IActionResult), StatusCodes.Status200OK)]
        [HttpPost("upload/{userId}")]
        //[Authorize(Roles = "InteriorDesigner,Admin")]  // allowed roles
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
                    imageUrl = imageUrl,
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
