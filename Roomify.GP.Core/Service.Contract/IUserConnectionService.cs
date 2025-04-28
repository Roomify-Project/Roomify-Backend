// Roomify.GP.Core/Services/Contract/IUserConnectionService.cs
using Roomify.GP.Core.Entities.Identity;

namespace Roomify.GP.Core.Services.Contract
{
    public interface IUserConnectionService
    {
        Task AddConnectionAsync(UserConnection connection);
        Task RemoveConnectionAsync(string connectionId);
    }
}
