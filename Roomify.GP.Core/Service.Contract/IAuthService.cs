using Roomify.GP.Core.DTOs.ApplicationUser;

using Roomify.GP.Core.Entities.Identity;


namespace Roomify.GP.Core.Service.Contract
{
    public interface IAuthService
    {
        Task<LoginResponseDto> RegisterAsync(UserCreateDto dto);
        Task<LoginResponseDto> LoginAsync(LoginDto dto);
        string GenerateJwtToken(ApplicationUser user);
        Task<bool> ForgetPasswordAsync(ForgetPasswordRequestDto request);
        Task<bool> ConfirmEmailAsync(string email, string otpCode);

        Task<bool> ResetPasswordAsync(ResetPasswordRequestDto request);

    }
}
