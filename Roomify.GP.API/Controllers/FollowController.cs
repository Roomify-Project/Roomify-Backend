using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Roomify.GP.Core.Service.Contract;
using System.Security.Claims;

namespace Roomify.GP.API.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class FollowController : ControllerBase
    {
        private readonly IFollowService _followService;

        public FollowController(IFollowService followService)
        {
            _followService = followService;
        }

        [HttpPost("{followingId}")]
        public async Task<IActionResult> Follow(Guid followingId)
        {
            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (currentUserId == null || followingId == Guid.Parse(currentUserId))
                return BadRequest("You can't follow yourself.");

            var success = await _followService.FollowAsync(Guid.Parse(currentUserId), followingId);

            if (!success)
                return BadRequest("You are already following this user.");

            return Ok(new { message = "Followed successfully" });

        }

        [HttpDelete("{followingId}")]
        public async Task<IActionResult> Unfollow(Guid followingId)
        {
            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (currentUserId == null)
                return Unauthorized();

            var success = await _followService.UnfollowAsync(Guid.Parse(currentUserId), followingId);

            if (!success)
                return BadRequest("You are not following this user.");

            return Ok(new { message = "Unfollowed successfully" });

        }

        [HttpGet("counts/{userId}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetFollowCounts(Guid userId)
        {
            var followers = await _followService.GetFollowersCountAsync(userId);
            var following = await _followService.GetFollowingCountAsync(userId);

            return Ok(new
            {
                Followers = followers,
                Following = following
            });
        }

        [HttpGet("followers/{userId}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetFollowers(Guid userId)
        {
            var followers = await _followService.GetFollowersAsync(userId);
            return Ok(followers);
        }

        [HttpGet("following/{userId}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetFollowing(Guid userId)
        {
            var following = await _followService.GetFollowingAsync(userId);
            return Ok(following);
        }


        [HttpGet("is-following/{targetUserId}")]
        public async Task<IActionResult> IsFollowing(Guid targetUserId)
        {
            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(currentUserId))
                return Unauthorized();

            var result = await _followService.IsFollowingAsync(Guid.Parse(currentUserId), targetUserId);
            return Ok(result);
        }

    }
}
