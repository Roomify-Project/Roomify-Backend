﻿using Microsoft.AspNetCore.Mvc;
using Roomify.GP.Core.DTOs.ApplicationUser;
using Roomify.GP.Core.Service.Contract;

namespace Roomify.GP.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] UserCreateDto dto)
        {
            var result = await _authService.RegisterAsync(dto);
            return Ok(result);
        }


        [HttpPost("confirm-email")]
        public async Task<IActionResult> ConfirmEmail([FromBody] ConfirmEmailDto dto)
        {
            var result = await _authService.ConfirmEmailAsync(dto.Email, dto.OtpCode);
            if (!result)
                return BadRequest("Invalid or expired code.");

            return Ok("Email confirmed successfully.");
        }



        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto dto)
        {
            var result = await _authService.LoginAsync(dto);
            return Ok(result);
        }

        [HttpPost("forget-password")]
        public async Task<IActionResult> ForgetPassword([FromBody] ForgetPasswordRequestDto request)
        {
            var success = await _authService.ForgetPasswordAsync(request);
            return success ? Ok("Reset link sent.") : NotFound("User not found");
        }

        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequestDto request)
        {
            var success = await _authService.ResetPasswordAsync(request);
            return success ? Ok("Password reset successfully.") : BadRequest("Invalid token or user.");
        }
    }
}