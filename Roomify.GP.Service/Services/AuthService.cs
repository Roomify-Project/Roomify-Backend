using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Roomify.GP.Core.DTOs.User;
using Roomify.GP.Core.Entities.Identity;
using Roomify.GP.Core.Service.Contract;
using Roomify.GP.Core.Settings;
using Roomify.GP.Core.DTOs.ApplicationUser;
using Roomify.GP.Core.Repositories.Contract;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Roomify.GP.Service
{
    public class AuthService : IAuthService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly JwtSettings _jwtSettings;
        private readonly IEmailService _emailService;
        private readonly IEmailConfirmationTokenRepository _emailConfirmationTokenRepository;
        private readonly IPendingRegistrationRepository _pendingRegistrationRepository;
        private readonly IJwtService _jwtService;

        public AuthService(
            UserManager<ApplicationUser> userManager,
            IOptions<JwtSettings> jwtOptions,
            IEmailService emailService,
            IEmailConfirmationTokenRepository emailTokenRepository,
            IPendingRegistrationRepository pendingRegistrationRepository,
            IJwtService jwtService)
        {
            _userManager = userManager;
            _jwtSettings = jwtOptions.Value;
            _emailService = emailService;
            _emailConfirmationTokenRepository = emailTokenRepository;
            _pendingRegistrationRepository = pendingRegistrationRepository;
            _jwtService = jwtService;
        }

        public async Task<LoginResponseDto> LoginAsync(LoginDto dto)
        {
            var user = await _userManager.FindByEmailAsync(dto.Email);
            if (user == null || !await _userManager.CheckPasswordAsync(user, dto.Password))
                throw new UnauthorizedAccessException("Invalid email or password.");

            var roles = await _userManager.GetRolesAsync(user);
            var token = user.EmailConfirmed ? await _jwtService.GenerateToken(user) : null;

            return new LoginResponseDto
            {
                UserId = user.Id,
                Email = user.Email,
                UserName = user.UserName,
                Roles = string.Join(", ", roles),
                Token = token, // لو الإيميل متأكد هيبعت توكن، لو مش متأكد هيبعت توكن فاضي
                RequiresEmailConfirmation = !user.EmailConfirmed, // هنا الصح
                Message = user.EmailConfirmed ? null : "Please confirm your email before logging in."
            };
        }


        public async Task<LoginResponseDto> RegisterAsync(UserCreateDto dto)
        {
            var existingUser = await _userManager.FindByEmailAsync(dto.Email);
            if (existingUser != null)
                throw new ApplicationException("User already exists with this email.");

            var pending = await _pendingRegistrationRepository.GetByEmailAsync(dto.Email);
            if (pending != null)
                throw new ApplicationException("You already have a pending registration. Please verify your email.");

            var newPendingRegistration = new PendingRegistration
            {
                FullName = dto.FullName,
                UserName = dto.UserName,
                Email = dto.Email,
                Bio = dto.Bio,
                Password = dto.Password,
                Roles = dto.Roles ?? "NormalUser",
                CreatedDate = DateTime.UtcNow,
                ExpiryDate = DateTime.UtcNow.AddHours(24)
            };

            await _pendingRegistrationRepository.AddAsync(newPendingRegistration);
            await _pendingRegistrationRepository.SaveChangesAsync();

            var otpCode = new Random().Next(100000, 999999).ToString();

            var emailToken = new EmailConfirmationToken
            {
                PendingRegistrationId = newPendingRegistration.Id,
                Code = otpCode,
                IsUsed = false,
                CreatedAt = DateTime.UtcNow,
                ExpiryAt = DateTime.UtcNow.AddMinutes(10)
            };

            await _emailConfirmationTokenRepository.AddAsync(emailToken);
            await _emailConfirmationTokenRepository.SaveChangesAsync();

            await _emailService.SendEmailAsync(dto.Email, "Roomify Email Confirmation", $"<h3>Confirm your email</h3><p>Your OTP is <b>{otpCode}</b></p>");

            return new LoginResponseDto
            {
                Email = dto.Email,
                UserName = dto.UserName,
                RequiresEmailConfirmation = true,
                Message = "Registration successful. Please check your email for verification code."
            };
        }

        public async Task<bool> ConfirmEmailAsync(string email, string otpCode)
        {
            var pending = await _pendingRegistrationRepository.GetByEmailAsync(email);
            if (pending == null)
                return false;

            var token = await _emailConfirmationTokenRepository.GetActiveTokenByEmailAndCodeAsync(email, otpCode);
            if (token == null || token.IsUsed || token.ExpiryAt < DateTime.UtcNow)
                return false;

            token.IsUsed = true;
            await _emailConfirmationTokenRepository.SaveChangesAsync();

            var user = new ApplicationUser
            {
                FullName = pending.FullName,
                UserName = pending.UserName,
                Email = pending.Email,
                Bio = pending.Bio,
                EmailConfirmed = true,
                CreatedDate = DateTime.UtcNow
            };

            var result = await _userManager.CreateAsync(user, pending.Password);
            if (!result.Succeeded)
                return false;

            await _userManager.AddToRoleAsync(user, pending.Roles);

            await _pendingRegistrationRepository.DeleteAsync(pending);
            await _pendingRegistrationRepository.SaveChangesAsync();

            return true;
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

            await _emailService.SendEmailAsync(user.Email, "Roomify – Password Reset OTP", $"<h3>Reset your password</h3><p>Your OTP is <b>{otpCode}</b></p>");

            return true;
        }

        public async Task<bool> ResetPasswordAsync(ResetPasswordRequestDto request)
        {
            var user = await _userManager.FindByEmailAsync(request.Email);
            if (user == null) return false;

            var token = await _emailConfirmationTokenRepository.GetActiveTokenAsync(user.Id, request.OtpCode);
            if (token == null || token.IsUsed || token.ExpiryAt < DateTime.UtcNow)
                return false;

            token.IsUsed = true;
            await _emailConfirmationTokenRepository.SaveChangesAsync();

            var removeResult = await _userManager.RemovePasswordAsync(user);
            if (!removeResult.Succeeded) return false;

            var addResult = await _userManager.AddPasswordAsync(user, request.NewPassword);
            if (!addResult.Succeeded) return false;

            await _emailService.SendEmailAsync(user.Email, "Roomify – Password Changed", "<h3>Your password has been successfully changed.</h3><p>If you did not request this, contact support.</p>");

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
