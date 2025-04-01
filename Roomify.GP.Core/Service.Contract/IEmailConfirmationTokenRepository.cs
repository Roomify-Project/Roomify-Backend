using Roomify.GP.Core.Entities.Identity;

public interface IEmailConfirmationTokenRepository
{
    Task<EmailConfirmationToken?> GetByUserIdAsync(Guid userId);
    Task AddAsync(EmailConfirmationToken token);
    void Remove(EmailConfirmationToken token);
    Task<EmailConfirmationToken?> GetActiveTokenAsync(string userId, string code);

    Task SaveChangesAsync();
}
