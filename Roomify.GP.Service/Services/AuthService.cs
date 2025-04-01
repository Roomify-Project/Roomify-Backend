using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Roomify.GP.Core.DTOs.User;
using Roomify.GP.Core.Entities.Identity;
using Roomify.GP.Core.Service.Contract;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System;
using System.Threading.Tasks;
using Roomify.GP.Core.DTOs.ApplicationUser;
using Roomify.GP.Core.Entities;
using Roomify.GP.Core.Repositories.Contract;

namespace Roomify.GP.Service
{
    public class AuthService : IAuthService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IConfiguration _configuration;
        private readonly IEmailService _emailService;
        private readonly IEmailConfirmationTokenRepository _emailConfirmationTokenRepository;

        public AuthService(
            UserManager<ApplicationUser> userManager,
            IConfiguration configuration,
            IEmailService emailService,
            IEmailConfirmationTokenRepository emailTokenRepository)
        {
            _userManager = userManager;
            _configuration = configuration;
            _emailService = emailService;
            _emailConfirmationTokenRepository = emailTokenRepository;
        }

        public async Task<LoginResponseDto> LoginAsync(LoginDto dto)
        {
            var user = await _userManager.FindByEmailAsync(dto.Email);
            if (user == null || !await _userManager.CheckPasswordAsync(user, dto.Password))
                throw new UnauthorizedAccessException("Invalid email or password.");

            var token = GenerateJwtToken(user);

            return new LoginResponseDto
            {
                UserName = user.UserName,
                Roles = user.Roles.ToString(),
                Token = token
            };
        }

        public async Task<LoginResponseDto> RegisterAsync(UserCreateDto dto)
        {
            var user = new ApplicationUser
            {
                FullName = dto.FullName,
                UserName = dto.UserName,
                Email = dto.Email,
                Bio = dto.Bio,
                CreatedDate = DateTime.UtcNow
            };

            var result = await _userManager.CreateAsync(user, dto.Password);

            if (!result.Succeeded)
            {
                var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                throw new ApplicationException($"Register failed: {errors}");
            }

            await _userManager.AddToRoleAsync(user, "User");

            // Generate 6-digit OTP
            var otpCode = new Random().Next(100000, 999999).ToString();

            var otpToken = new EmailConfirmationToken
            {
                UserId = user.Id,
                Code = otpCode,
                IsUsed = false,
                CreatedAt = DateTime.UtcNow,
                ExpiryAt = DateTime.UtcNow.AddMinutes(10)
            };

            await _emailConfirmationTokenRepository.AddAsync(otpToken);
            await _emailConfirmationTokenRepository.SaveChangesAsync();

            // Send confirmation email
            var subject = "Roomify Email Confirmation";
            var body = $"<h3>Verify your email</h3><p>Your confirmation code is: <b>{otpCode}</b></p>";
            await _emailService.SendEmailAsync(user.Email, subject, body);

            var token = GenerateJwtToken(user);

            return new LoginResponseDto
            {
                UserId = user.Id,
                Email = user.Email,
                UserName = user.UserName,
                Token = token
            };
        }

        public async Task<bool> ForgetPasswordAsync(ForgetPasswordRequestDto request)
        {
            var user = await _userManager.FindByEmailAsync(request.Email);
            if (user == null) return false;

            // Generate OTP
            var otpCode = new Random().Next(100000, 999999).ToString();

            var otpToken = new EmailConfirmationToken
            {
                UserId = user.Id,
                Code = otpCode,
                IsUsed = false,
                CreatedAt = DateTime.UtcNow,
                ExpiryAt = DateTime.UtcNow.AddMinutes(10)
            };

            await _emailConfirmationTokenRepository.AddAsync(otpToken);
            await _emailConfirmationTokenRepository.SaveChangesAsync();

            // Send Email with OTP
            var subject = "Roomify – Password Reset OTP";
            var body = $"<h3>Reset your password</h3><p>Your OTP is: <b>{otpCode}</b></p>";

            await _emailService.SendEmailAsync(user.Email, subject, body);

            return true;
        }


        public async Task<bool> ResetPasswordAsync(ResetPasswordRequestDto request)
        {
            var user = await _userManager.FindByEmailAsync(request.Email);
            if (user == null) return false;

            var token = await _emailConfirmationTokenRepository.GetActiveTokenAsync(user.Id, request.OtpCode);
            if (token == null || token.IsUsed || token.ExpiryAt < DateTime.UtcNow) return false;

            token.IsUsed = true;
            await _emailConfirmationTokenRepository.SaveChangesAsync();

            // Remove old password then set new one
            var removeResult = await _userManager.RemovePasswordAsync(user);
            if (!removeResult.Succeeded) return false;

            var addResult = await _userManager.AddPasswordAsync(user, request.NewPassword);
            if (!addResult.Succeeded) return false;

            // Send Confirmation Email
            var subject = "Roomify – Password Changed";
            var body = "<h3>Your password has been successfully changed.</h3><p>If you did not request this change, please contact support immediately.</p>";
            await _emailService.SendEmailAsync(user.Email, subject, body);

            return true;
        }


        public async Task<bool> ConfirmEmailAsync(string email, string otpCode)
        {
            var user = await _userManager.FindByEmailAsync(email);
            if (user == null) return false;

            var token = await _emailConfirmationTokenRepository.GetActiveTokenAsync(user.Id, otpCode);
            if (token == null) return false;

            token.IsUsed = true;
            await _emailConfirmationTokenRepository.SaveChangesAsync();

            user.EmailConfirmed = true;
            await _userManager.UpdateAsync(user);

            return true;
        }

        private string GenerateJwtToken(ApplicationUser user)
        {
            var authClaims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id),
                new Claim(ClaimTypes.Name, user.UserName),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            var authKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["JwtSettings:SecretKey"]));
            var token = new JwtSecurityToken(
                issuer: _configuration["JwtSettings:Issuer"],
                audience: _configuration["JwtSettings:Audience"],
                expires: DateTime.UtcNow.AddHours(12),
                claims: authClaims,
                signingCredentials: new SigningCredentials(authKey, SecurityAlgorithms.HmacSha256)
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        string IAuthService.GenerateJwtToken(ApplicationUser user)
        {
            return GenerateJwtToken(user);
        }
    }
}
