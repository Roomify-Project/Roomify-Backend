using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Roomify.GP.Core.Entities.Identity;
using Roomify.GP.Core.Service.Contract;
using Roomify.GP.Core.Settings;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Roomify.GP.Service.Services
{
    public class JwtService : IJwtService
    {
        private readonly JwtSettings _jwtSettings;
        private readonly UserManager<ApplicationUser> _userManager;

        public JwtService(IOptions<JwtSettings> jwtOptions, UserManager<ApplicationUser> userManager)
        {
            _jwtSettings = jwtOptions.Value;
            _userManager = userManager;
        }

        public async Task<string> GenerateToken(ApplicationUser user)
        {
          
            var roles = await _userManager.GetRolesAsync(user);
            Console.WriteLine(string.Join(", ", roles)); // ✅ هنا اطبع الرولز

            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
                new Claim(JwtRegisteredClaimNames.Email, user.Email),
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.UserName ?? "")
            };

            foreach (var role in roles)
            {
                // كلايمز مزدوجة عشان نضمن التوافق مع جميع السيناريوهات
                claims.Add(new Claim(ClaimTypes.Role, role));
                claims.Add(new Claim("role", role));
            }

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.SecretKey));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _jwtSettings.Issuer,
                audience: _jwtSettings.Audience,
                claims: claims,
                expires: DateTime.Now.AddDays(_jwtSettings.ExpiryInDays),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
