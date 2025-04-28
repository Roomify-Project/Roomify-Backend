// Roomify.GP.Core/Repositories/Contract/IUserConnectionRepository.cs
using Roomify.GP.Core.Entities.Identity;

namespace Roomify.GP.Core.Repositories.Contract
{
    public interface IUserConnectionRepository
    {
        Task AddAsync(UserConnection connection);
        Task RemoveAsync(string connectionId);
        Task<List<UserConnection>> GetByUserIdAsync(Guid userId);

    }
}
