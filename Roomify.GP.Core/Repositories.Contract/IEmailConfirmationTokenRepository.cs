using Roomify.GP.Core.Entities.Identity;

namespace Roomify.GP.Core.Repositories.Contract
{
    public interface IEmailConfirmationTokenRepository
    {
        Task<EmailConfirmationToken?> GetActiveTokenAsync(string userId, string code);
        Task AddAsync(EmailConfirmationToken token);
        Task Remove(EmailConfirmationToken token);
        Task<EmailConfirmationToken?> GetByUserIdAsync(Guid userId);
        Task SaveChangesAsync();
    }
}
