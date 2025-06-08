using Microsoft.AspNetCore.Mvc;
using Roomify.GP.Core.Service.Contract;
using Roomify.GP.Core.DTOs.ApplicationUser;
using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Roomify.GP.Core.Entities.Identity;
using Roomify.GP.Core.Services.Contract;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace Roomify.GP.API.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class UsersController : ControllerBase
    {
        private readonly IUserService _userService;

        public UsersController(IUserService userService)
        {
            _userService = userService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllUsers()
        {
            var users = await _userService.GetAllUsersAsync();
            return Ok(users);
        }

        [HttpGet("{id:guid}")]
        [ProducesResponseType(typeof(ApplicationUser), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetUserById(Guid id)
        {
            var user = await _userService.GetUserByIdAsync(id);
            return user is null ? NotFound() : Ok(user);
        }

        [HttpPut("update-profile")]
        public async Task<IActionResult> UpdateProfile([FromForm] UserUpdateDto dto)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!Guid.TryParse(userId, out var parsedUserId))
                return Unauthorized();

            var profileImageUrl = await _userService.UpdateUserAsync(parsedUserId, dto);

            var updatedUser = await _userService.GetUserByIdAsync(parsedUserId);
            return Ok(new
            {
                message = "User profile updated successfully.",
                user = updatedUser
            });

        }

        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> DeleteUser(Guid id)
        {
            var user = await _userService.GetUserByIdAsync(id);
            if (user == null) return NotFound();

            if (!user.EmailConfirmed)
                return BadRequest("Cannot delete user before confirming email.");

            var result = await _userService.DeleteUserAsync(id);
            return result ? Ok("User deleted successfully") : NotFound();
            
        }
        [HttpGet("search")]
        public async Task<IActionResult> SearchUsers([FromQuery] string query)
        {
            if (string.IsNullOrWhiteSpace(query))
                return BadRequest("Search query is required.");

            var results = await _userService.SearchUsersAsync(query);
            return Ok(results);
        }

    }
}
