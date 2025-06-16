using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Roomify.GP.Core.DTOs.PortfolioPost;
using Roomify.GP.Core.Entities.Identity;
using Roomify.GP.Core.Entities;
using Roomify.GP.Core.Service.Contract;
using System.Security.Claims;
using Roomify.GP.API.Errors;

[Authorize]
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

    public PortfolioPostController(
        IPortfolioPostService portfolioPostService,
        ICloudinaryService cloudinaryService,
        IMapper mapper,
        UserManager<ApplicationUser> userManager,
        ILogger<PortfolioPostController> logger,
        IRoomImageService roomImageService)
    {
        _portfolioPostService = portfolioPostService;
        _cloudinaryService = cloudinaryService;
        _mapper = mapper;
        _userManager = userManager;
        _logger = logger;
        _roomImageService = roomImageService;
    }

    private Guid GetCurrentUserId()
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return Guid.TryParse(userId, out var guid) ? guid : Guid.Empty;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var userId = GetCurrentUserId(); // ✅ Use actual user ID
        var (posts, designs) = await _portfolioPostService.GetAllCombinedAsync(userId);
        return Ok(new { PortfolioPosts = posts, SavedDesigns = designs });
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        try
        {
            var userId = GetCurrentUserId(); // ✅ Use actual user ID
            var result = await _portfolioPostService.GetByIdCombinedAsync(id, userId);
            return Ok(new { Type = result.Type, Data = result.Data });
        }
        catch (KeyNotFoundException)
        {
            return NotFound(new ApiErrorResponse(404, "Item not found"));
        }
    }

    [HttpGet("by-user/{userId}")]
    public async Task<IActionResult> GetByUser(Guid userId)
    {
        var response = await _portfolioPostService.GetByUserIdAsync(userId);
        return Ok(response);
    }

    [HttpPost("upload")]
    public async Task<IActionResult> Upload([FromForm] PortfolioPostDto portfolioPostDto)
    {
        var userId = GetCurrentUserId();
        var user = await _userManager.FindByIdAsync(userId.ToString());
        if (user == null)
            return NotFound(new ApiErrorResponse(404, "User not found."));

        if (portfolioPostDto.ImageFile == null || portfolioPostDto.ImageFile.Length == 0)
            return BadRequest(new ApiErrorResponse(400, "Image is required."));

        var imageUrl = await _cloudinaryService.UploadImageAsync(portfolioPostDto.ImageFile);

        var post = _mapper.Map<PortfolioPost>(portfolioPostDto);
        post.Id = Guid.NewGuid();
        post.ImagePath = imageUrl;
        post.CreatedAt = DateTime.UtcNow;
        post.ApplicationUserId = userId;

        await _portfolioPostService.AddAsync(userId, post);
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

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        if (id == Guid.Empty)
            return BadRequest(new ApiErrorResponse(400));

        var post = await _portfolioPostService.GetByIdAsync(id);
        if (post == null)
            return NotFound(new ApiErrorResponse(404));

        try { await _cloudinaryService.DeleteImageAsync(post.ImagePath); } catch { }

        await _portfolioPostService.DeleteAsync(id);
        return Ok(new { message = "Post and image deleted successfully." });
    }

    [HttpPost("{id}/likes")]
    public async Task<IActionResult> Like(Guid id, [FromQuery] bool isPost)
    {
        var userId = GetCurrentUserId();
        var like = await _portfolioPostService.AddLikeAsync(id, isPost, userId);
        return Ok(like);
    }

    [HttpDelete("{id}/likes")]
    public async Task<IActionResult> Unlike(Guid id, [FromQuery] bool isPost)
    {
        var userId = GetCurrentUserId();
        await _portfolioPostService.RemoveLikeAsync(id, isPost, userId);
        return NoContent();
    }
}
