using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Roomify.GP.Core.DTOs.User;
using Roomify.GP.Core.Entities.Identity;
using Roomify.GP.Core.Service.Contract;
using Roomify.GP.Core.Settings;
using Roomify.GP.Core.DTOs.ApplicationUser;
using Roomify.GP.Core.Repositories.Contract;

namespace Roomify.GP.Service
{
    public class AuthService : IAuthService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly JwtSettings _jwtSettings;
        private readonly IEmailService _emailService;
        private readonly IEmailConfirmationTokenRepository _emailConfirmationTokenRepository;
        private readonly IJwtService _jwtService;

        public AuthService(
            UserManager<ApplicationUser> userManager,
            IOptions<JwtSettings> jwtOptions,
            IEmailService emailService,
            IEmailConfirmationTokenRepository emailTokenRepository,
            IJwtService jwtService)
        {
            _userManager = userManager;
            _jwtSettings = jwtOptions.Value;
            _emailService = emailService;
            _emailConfirmationTokenRepository = emailTokenRepository;
            _jwtService = jwtService;
        }

        public async Task<LoginResponseDto> LoginAsync(LoginDto dto)
        {
            var user = await _userManager.FindByEmailAsync(dto.Email);
            if (user == null || !await _userManager.CheckPasswordAsync(user, dto.Password))
                throw new UnauthorizedAccessException("Invalid email or password.");

            var roles = await _userManager.GetRolesAsync(user);
            Console.WriteLine("🧾 Roles: " + string.Join(", ", roles));
            var token = await _jwtService.GenerateToken(user);

            return new LoginResponseDto
            {
                UserId = user.Id,
                Email = user.Email,
                UserName = user.UserName,
                Roles = string.Join(", ", roles),
                Token = token
            };
        }

        public async Task<LoginResponseDto> RegisterAsync(UserCreateDto dto)
        {
            var existingUser = await _userManager.FindByEmailAsync(dto.Email);
            if (existingUser != null)
                throw new ApplicationException("User already exists with this email.");

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
            var allowedRoles = new[] { "NormalUser", "InteriorDesigner" };

            if (!allowedRoles.Contains(dto.Roles))
                throw new ApplicationException("Invalid role. You can only choose 'NormalUser' or 'InteriorDesigner'.");

            await _userManager.AddToRoleAsync(user, dto.Roles);

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

            var subject = "Roomify Email Confirmation";
            var body = $"<h3>Verify your email</h3><p>Your confirmation code is: <b>{otpCode}</b></p>";
            await _emailService.SendEmailAsync(user.Email, subject, body);

            var roles = await _userManager.GetRolesAsync(user);
            var token = await _jwtService.GenerateToken(user);

            return new LoginResponseDto
            {
                UserId = user.Id,
                Email = user.Email,
                UserName = user.UserName,
                Roles = string.Join(", ", roles),
                Token = token
            };
        }

        public async Task<bool> ForgetPasswordAsync(ForgetPasswordRequestDto request)
        {
            var user = await _userManager.FindByEmailAsync(request.Email);
            if (user == null) return false;

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

            var removeResult = await _userManager.RemovePasswordAsync(user);
            if (!removeResult.Succeeded) return false;

            var addResult = await _userManager.AddPasswordAsync(user, request.NewPassword);
            if (!addResult.Succeeded) return false;

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
        public async Task<bool> ChangePasswordAsync(Guid userId, ChangePasswordDto dto)
        {
            if (dto.NewPassword != dto.ConfirmNewPassword)
                return false;

            var user = await _userManager.FindByIdAsync(userId.ToString());
            if (user == null) return false;

            var result = await _userManager.ChangePasswordAsync(user, dto.CurrentPassword, dto.NewPassword);

            return result.Succeeded;
        }

    }
}
