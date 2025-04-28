using Microsoft.EntityFrameworkCore;
using Roomify.GP.Core.Entities.Identity;
using Roomify.GP.Core.Repositories.Contract;
using Roomify.GP.Repository.Data.Contexts;

namespace Roomify.GP.Repository.Repositories
{
    public class EmailConfirmationTokenRepository : IEmailConfirmationTokenRepository
    {
        private readonly AppDbContext _context;

        public EmailConfirmationTokenRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task AddAsync(EmailConfirmationToken token)
        {
            await _context.EmailConfirmationTokens.AddAsync(token);
        }

        public async Task<EmailConfirmationToken?> GetActiveTokenAsync(Guid userId, string code)
        {
            return await _context.EmailConfirmationTokens
                .Where(t => t.UserId == userId && t.Code == code && !t.IsUsed && t.ExpiryAt > DateTime.UtcNow)
                .FirstOrDefaultAsync();
        }

        public async Task<EmailConfirmationToken?> GetByUserIdAsync(Guid userId)
        {
            return await _context.EmailConfirmationTokens
                .Where(t => t.UserId == userId && !t.IsUsed && t.ExpiryAt > DateTime.UtcNow)
                .FirstOrDefaultAsync();
        }
        public async Task<EmailConfirmationToken> GetActiveTokenByEmailAndCodeAsync(string email, string otpCode)
        {
            return await _context.EmailConfirmationTokens
                .Include(t => t.PendingRegistration) // عشان تربط بالبندنج
                .Where(t => !t.IsUsed && t.ExpiryAt > DateTime.UtcNow)
                .FirstOrDefaultAsync(t => t.PendingRegistration.Email.ToLower() == email.ToLower() && t.Code == otpCode);
        }



        public void Remove(EmailConfirmationToken token)
        {
            _context.EmailConfirmationTokens.Remove(token);
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}
